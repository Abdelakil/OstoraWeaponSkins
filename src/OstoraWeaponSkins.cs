using System.Collections.Concurrent;
using System.Linq;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SchemaDefinitions;

using OstoraWeaponSkins.Econ;

namespace OstoraWeaponSkins;

[PluginMetadata(
    Id = "OstoraWeaponSkins",
    Version = "1.0.0",
    Name = "OSTORA WeaponSkins",
    Author = "ostora",
    Description = "Simplified weapon skins plugin for CS2 — handles weapons, knives, gloves, agents, music."
)]
public class OstoraWeaponSkins : BasePlugin
{
    private ILogger Logger { get; set; } = null!;
    private Natives Native { get; set; } = null!;
    private Database Db { get; set; } = null!;

    // ── SO-cache readiness set. We no longer trust the hook-side CCSPlayerInventory
    // instance for mutations (it can get confused if the native pointer is reused
    // between player slots). Instead, we use this purely as a "is this player's
    // SOCache currently subscribed?" signal, and we *always* derive the actual
    // CCSPlayerInventory for mutations directly from that player's live
    // CCSPlayerController.InventoryServices pointer — guaranteeing the inventory
    // we write to really belongs to the player we queried the DB for.
    private readonly ConcurrentDictionary<ulong, byte> _subscribedSteamIds = new();

    // ── Agent models (parsed from items_game.txt) ───────────────
    private static readonly Dictionary<int, string> AgentModels = new();

    // ── Default models per player (stores default model for agent reset; NOT a skin cache) ──
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, string>> _defaultModels = new();

    // ── Per-player monotonic load epoch.
    // Every new async load bumps this counter for the player; scheduled callbacks
    // that finish after a newer load started are ignored. This prevents stale DB
    // responses from being applied to the wrong player-state, without introducing
    // any skin cache.
    private readonly ConcurrentDictionary<ulong, long> _loadEpochs = new();

    // ── Per-player auto-refresh flag (replaces the previous single shared field,
    // which was not safe with multiple concurrent refreshes).
    private readonly ConcurrentDictionary<ulong, byte> _autoRefreshFlags = new();

    public OstoraWeaponSkins(ISwiftlyCore core) : base(core) { }

    // ================================================================
    //  LOAD
    // ================================================================
    public override void Load(bool hotReload)
    {
        Logger = Core.Logger;

        Native = new Natives(Core, Logger);
        Db = new Database(Core);
        Db.Start(Core.Database);

        // Parse econ data from items_game.txt
        EconParser.ParseAll(Core, Logger);

        // Initialize agent models from parsed data
        foreach (var (index, model) in EconParser.ParsedData.AgentModels)
        {
            AgentModels[index] = model;
        }

        // SOCache hooks → just mark the player as "their cache is subscribed".
        // We intentionally do NOT store the CCSPlayerInventory instance passed
        // to the hook: under concurrent subscribes across multiple players the
        // captured wrapper has been observed to end up keyed incorrectly and
        // cause cross-player skin application. We derive a fresh inventory
        // straight from controller.InventoryServices at apply time instead.
        Native.OnSOCacheSubscribed += (_inv, soid) =>
        {
            _subscribedSteamIds[soid.SteamID] = 1;
            Logger.LogInformation("[OSTORA] SOCache subscribed for {SteamID}", soid.SteamID);
        };
        Native.OnSOCacheUnsubscribed += (_inv, soid) =>
        {
            _subscribedSteamIds.TryRemove(soid.SteamID, out _);
            _loadEpochs.TryRemove(soid.SteamID, out _);
            _autoRefreshFlags.TryRemove(soid.SteamID, out _);
            _defaultModels.TryRemove(soid.SteamID, out _);
            Logger.LogInformation("[OSTORA] SOCache unsubscribed for {SteamID}", soid.SteamID);
        };

        // GiveNamedItem hook → apply weapon/knife attributes on pickup
        Native.OnGiveNamedItemPost += OnGiveNamedItemPost;

        // Player spawn → apply gloves, agents, refresh weapons
        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);

        // Team change → re-apply music kit for new team
        Core.GameEvent.HookPost<EventPlayerTeam>(OnPlayerTeam);

        // RCON command: ws_refreshskins <steamid64>
        Core.Command.RegisterCommand("ws_refreshskins", OnRefreshCommand, registerRaw: true);

        Logger.LogInformation("[OSTORA] Loaded.");

        // Hot reload: apply to existing players
        if (hotReload)
        {
            foreach (var player in Core.PlayerManager.GetAllPlayers())
            {
                if (player.Controller is { IsValid: true, InventoryServices.IsValid: true })
                    LoadAndApplyPlayer(player);
            }
        }
    }

    public override void Unload() { }

    // ================================================================
    //  HELPERS
    // ================================================================

    /// <summary>Bump the per-player load epoch and return the new value.</summary>
    private long BumpEpoch(ulong steamId) =>
        _loadEpochs.AddOrUpdate(steamId, 1L, (_, v) => v + 1);

    /// <summary>True if the given epoch is still the latest for the player.</summary>
    private bool IsCurrentEpoch(ulong steamId, long epoch) =>
        _loadEpochs.TryGetValue(steamId, out var v) && v == epoch;

    /// <summary>Re-resolve an online player by SteamID on the game thread.</summary>
    private IPlayer? GetPlayerBySteamId(ulong steamId)
    {
        foreach (var p in Core.PlayerManager.GetAllPlayers())
        {
            if (p.SteamID == steamId) return p;
        }
        return null;
    }

    /// <summary>
    /// Derive the CCSPlayerInventory for a player DIRECTLY from their own
    /// controller.InventoryServices pointer + the native offset. This is the
    /// only guaranteed-correct way to obtain that player's inventory without
    /// risking a stale/reused wrapper from a dictionary. Also verifies that
    /// the derived inventory's SOCache.Owner.SteamID actually matches the
    /// requested player — a hard sanity check against cross-player writes.
    /// </summary>
    private bool TryGetPlayerInventory(IPlayer player, out CCSPlayerInventory inventory)
    {
        inventory = null!;
        try
        {
            var controller = player.Controller;
            if (controller is not { IsValid: true }) return false;
            var services = controller.InventoryServices;
            if (services is not { IsValid: true }) return false;

            var inv = new CCSPlayerInventory(
                services.Address + Native.CCSPlayerController_InventoryServices_m_pInventoryOffset);
            if (!inv.IsValid) return false;

            // Cross-check: the inventory's SOCache owner MUST be the player.
            // If it doesn't match, something is very wrong — refuse to write.
            if (inv.SteamID != player.SteamID)
            {
                Logger.LogWarning(
                    "[OSTORA] Inventory SteamID mismatch for player {Expected}: got {Actual}. Refusing to apply.",
                    player.SteamID, inv.SteamID);
                return false;
            }

            inventory = inv;
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[OSTORA] TryGetPlayerInventory failed for {SteamID}", player.SteamID);
            return false;
        }
    }

    /// <summary>
    /// Verify that a weapon is still owned by the expected player/team.
    /// Used to guard against applying a skin to a weapon that has been
    /// re-assigned between the async DB fetch and the world-update apply.
    /// </summary>
    private static bool VerifyWeaponOwner(CBasePlayerWeapon weapon, ulong expectedSteamId, Team expectedTeam)
    {
        try
        {
            if (!weapon.IsValid) return false;
            var ownerHandle = weapon.OwnerEntity;
            if (!ownerHandle.IsValid) return false;
            var pawn = ownerHandle.Value?.As<CCSPlayerPawn>();
            if (pawn == null || !pawn.IsValid) return false;
            var controllerHandle = pawn.Controller;
            if (!controllerHandle.IsValid) return false;
            var controller = controllerHandle.Value?.As<CCSPlayerController>();
            if (controller == null || !controller.IsValid) return false;
            return controller.SteamID == expectedSteamId && controller.Team == expectedTeam;
        }
        catch
        {
            return false;
        }
    }

    // ================================================================
    //  PLAYER SPAWN
    // ================================================================
    private HookResult OnPlayerSpawn(EventPlayerSpawn ev)
    {
        var player = ev.UserIdPlayer;
        if (player == null || !player.IsAlive) return HookResult.Continue;

        // Always load from DB on spawn - NO CACHE
        LoadAndApplyPlayer(player);

        return HookResult.Continue;
    }

    // ================================================================
    //  PLAYER TEAM CHANGE
    // ================================================================
    private HookResult OnPlayerTeam(EventPlayerTeam ev)
    {
        var player = ev.UserIdPlayer;
        if (player == null || ev.Disconnect) return HookResult.Continue;

        // Always reload from DB on team change - NO CACHE
        LoadAndApplyPlayer(player);

        return HookResult.Continue;
    }

    // ================================================================
    //  LOAD FROM DB + APPLY (NO CACHE - always from DB)
    // ================================================================
    private void LoadAndApplyPlayer(IPlayer player)
    {
        // Capture ONLY the SteamID across async boundaries. The IPlayer reference
        // is re-resolved on the game thread inside scheduled callbacks to avoid
        // acting on a stale or reused handle that could point to another player.
        var steamId = player.SteamID;
        var epoch = BumpEpoch(steamId);

        Task.Run(async () =>
        {
            try
            {
                // ALWAYS fetch directly from database - NO CACHING.
                // Each call is keyed strictly by steamId → no cross-player leakage.
                var weapons = await Db.GetWeaponSkinsAsync(steamId);
                var knives  = await Db.GetKnifeSkinsAsync(steamId);
                var gloves  = await Db.GetGloveSkinsAsync(steamId);
                var agents  = await Db.GetAgentsAsync(steamId);
                var music   = await Db.GetMusicKitAsync(steamId);

                // Defensive: drop any row whose SteamID does not match (should never
                // happen with correct queries, but guards against future bugs).
                weapons = weapons.Where(w => w.SteamID == steamId).ToList();
                knives  = knives .Where(k => k.SteamID == steamId).ToList();
                gloves  = gloves .Where(g => g.SteamID == steamId).ToList();
                music   = music  .Where(m => m.SteamID == steamId).ToList();

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!IsCurrentEpoch(steamId, epoch)) return;

                    var p = GetPlayerBySteamId(steamId);
                    if (p == null) return;
                    if (p.Controller is not { IsValid: true }) return;

                    if (!_subscribedSteamIds.ContainsKey(steamId))
                    {
                        Logger.LogWarning("[OSTORA] SOCache not yet subscribed for {SteamID} on spawn — skipping load", steamId);
                        return;
                    }

                    if (!TryGetPlayerInventory(p, out var inv))
                    {
                        Logger.LogWarning("[OSTORA] Could not derive inventory for {SteamID} on spawn", steamId);
                        return;
                    }

                    var team = p.Controller.Team;

                    // Narrow to current team, strictly per-player.
                    var teamWeapons = weapons.Where(w => w.Team == team).ToList();
                    var teamKnives  = knives .Where(k => k.Team == team).ToList();
                    var teamGloves  = gloves .Where(g => g.Team == team).ToList();
                    var teamMusic   = music  .FirstOrDefault(m => m.Team == team);

                    Logger.LogInformation(
                        "[OSTORA] Applying for {SteamID} team={Team}: {Weapons} weapons, {Knives} knives, {Gloves} gloves, music={Music}",
                        steamId, team, teamWeapons.Count, teamKnives.Count, teamGloves.Count, teamMusic?.MusicID ?? -1);

                    // Update inventory loadout for current team only
                    foreach (var w in teamWeapons) inv.UpdateWeaponSkin(w);
                    foreach (var k in teamKnives)  inv.UpdateKnifeSkin(k);
                    foreach (var g in teamGloves)  inv.UpdateGloveSkin(g);
                    if (teamMusic != null) inv.UpdateMusicKit(teamMusic.MusicID);

                    // Apply visuals after inventory updates settle (2 ticks)
                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (!IsCurrentEpoch(steamId, epoch)) return;
                            var p2 = GetPlayerBySteamId(steamId);
                            if (p2 == null || !p2.IsAlive) return;

                            var team2 = p2.Controller.Team;

                            // Re-filter in case team changed since the outer callback.
                            var applyWeapons = weapons.Where(w => w.Team == team2).ToList();
                            var applyKnives  = knives .Where(k => k.Team == team2).ToList();
                            var applyGlove   = gloves .FirstOrDefault(g => g.Team == team2);
                            var applyAgent   = agents .FirstOrDefault(a => a.Team == team2);

                            ApplyWeaponAttributesFromData(p2, applyWeapons, applyKnives, team2);

                            if (applyGlove != null && TryGetPlayerInventory(p2, out var inv2))
                                ApplyGloveVisualFromData(p2, applyGlove, inv2, team2);

                            if (applyAgent.AgentIndex != 0)
                                ApplyAgentVisualFromData(p2, applyAgent.AgentIndex);

                            Logger.LogInformation("[OSTORA] Spawn load complete for {SteamID} (Team: {Team})", steamId, team2);

                            // Auto-refresh after 0.5s to fix glove textures
                            Core.Scheduler.DelayBySeconds(0.5f, () =>
                            {
                                if (!IsCurrentEpoch(steamId, epoch)) return;
                                var p3 = GetPlayerBySteamId(steamId);
                                if (p3 == null || !p3.IsAlive) return;
                                if (!TryGetPlayerInventory(p3, out var inv3)) return;
                                var team3 = p3.Controller.Team;
                                var freshGlove = gloves.FirstOrDefault(g => g.Team == team3);
                                if (freshGlove != null)
                                    ApplyGloveVisualFromData(p3, freshGlove, inv3, team3);
                            });
                        });
                    });
                });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "[OSTORA] Failed to load data for {SteamID}", steamId);
            }
        });
    }

    // ================================================================
    //  RCON REFRESH (NO CACHE - always from DB)
    // ================================================================
    private void OnRefreshCommand(SwiftlyS2.Shared.Commands.ICommandContext context)
    {
        if (context.Args.Length < 1) return;
        if (!ulong.TryParse(context.Args[0], out var steamId))
        {
            Logger.LogWarning("[OSTORA] Invalid SteamID: {Arg}", context.Args[0]);
            return;
        }

        // Atomic per-player auto-refresh check.
        var isAuto = _autoRefreshFlags.TryRemove(steamId, out _);

        Logger.LogInformation("[OSTORA] Refresh requested for {SteamID} (Auto: {IsAuto})", steamId, isAuto);

        var epoch = BumpEpoch(steamId);

        Task.Run(async () =>
        {
            try
            {
                var weapons = await Db.GetWeaponSkinsAsync(steamId);
                var knives  = await Db.GetKnifeSkinsAsync(steamId);
                var gloves  = await Db.GetGloveSkinsAsync(steamId);
                var agents  = await Db.GetAgentsAsync(steamId);
                var music   = await Db.GetMusicKitAsync(steamId);

                weapons = weapons.Where(w => w.SteamID == steamId).ToList();
                knives  = knives .Where(k => k.SteamID == steamId).ToList();
                gloves  = gloves .Where(g => g.SteamID == steamId).ToList();
                music   = music  .Where(m => m.SteamID == steamId).ToList();

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!IsCurrentEpoch(steamId, epoch)) return;

                    var player = GetPlayerBySteamId(steamId);
                    if (player == null)
                    {
                        Logger.LogWarning("[OSTORA] Player {SteamID} not found online", steamId);
                        return;
                    }

                    if (!_subscribedSteamIds.ContainsKey(steamId))
                    {
                        Logger.LogWarning("[OSTORA] SOCache not yet subscribed for {SteamID}", steamId);
                        return;
                    }

                    if (!TryGetPlayerInventory(player, out var inv))
                    {
                        Logger.LogWarning("[OSTORA] Could not derive inventory for {SteamID}", steamId);
                        return;
                    }

                    var team = player.Controller.Team;

                    var teamWeapons = weapons.Where(w => w.Team == team).ToList();
                    var teamKnives  = knives .Where(k => k.Team == team).ToList();
                    var teamGloves  = gloves .Where(g => g.Team == team).ToList();
                    var teamMusic   = music  .FirstOrDefault(m => m.Team == team);

                    foreach (var w in teamWeapons) inv.UpdateWeaponSkin(w);
                    foreach (var k in teamKnives)  inv.UpdateKnifeSkin(k);
                    foreach (var g in teamGloves)  inv.UpdateGloveSkin(g);
                    if (teamMusic != null) inv.UpdateMusicKit(teamMusic.MusicID);

                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (!IsCurrentEpoch(steamId, epoch)) return;
                            var p2 = GetPlayerBySteamId(steamId);
                            if (p2 == null || !p2.IsAlive) return;

                            var team2 = p2.Controller.Team;

                            var applyWeapons = weapons.Where(w => w.Team == team2).ToList();
                            var applyKnives  = knives .Where(k => k.Team == team2).ToList();
                            var applyGlove   = gloves .FirstOrDefault(g => g.Team == team2);
                            var applyAgent   = agents .FirstOrDefault(a => a.Team == team2);

                            ApplyWeaponAttributesFromData(p2, applyWeapons, applyKnives, team2);

                            if (applyGlove != null && TryGetPlayerInventory(p2, out var inv2))
                                ApplyGloveVisualFromData(p2, applyGlove, inv2, team2);

                            if (applyAgent.AgentIndex != 0)
                                ApplyAgentVisualFromData(p2, applyAgent.AgentIndex);

                            Logger.LogInformation("[OSTORA] Refresh complete for {SteamID}", steamId);

                            // Auto-refresh after 0.5s to fix glove textures (only if this
                            // invocation was not itself an auto-refresh).
                            if (!isAuto)
                            {
                                Core.Scheduler.DelayBySeconds(0.5f, () =>
                                {
                                    _autoRefreshFlags[steamId] = 1;
                                    Core.Engine.ExecuteCommand($"ws_refreshskins {steamId}");
                                });
                            }
                        });
                    });
                });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "[OSTORA] Refresh failed for {SteamID}", steamId);
            }
        });
    }

    // ================================================================
    //  GIVE NAMED ITEM POST → apply skin attributes on weapon pickup (NO CACHE - query DB)
    // ================================================================
    private void OnGiveNamedItemPost(CCSPlayer_ItemServices services, CBasePlayerWeapon weapon)
    {
        ulong steamId;
        Team team;
        ushort defIndex;

        // Resolve the owning player ONCE on the game thread, then capture only
        // value types across the async boundary. The weapon reference itself is
        // re-verified before we apply anything to avoid cross-player contamination
        // if the weapon changes owner while the DB query is in flight.
        try
        {
            var ownerHandle = weapon.OwnerEntity;
            if (!ownerHandle.IsValid) return;
            var owner = ownerHandle.Value?.As<CCSPlayerPawn>();
            if (owner == null || !owner.IsValid) return;
            var controllerHandle = owner.Controller;
            if (!controllerHandle.IsValid) return;
            var controller = controllerHandle.Value?.As<CCSPlayerController>();
            if (controller == null || !controller.IsValid) return;

            steamId  = controller.SteamID;
            team     = controller.Team;
            defIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[OSTORA] GiveNamedItemPost resolve error");
            return;
        }

        var weaponRef = weapon;

        Task.Run(async () =>
        {
            try
            {
                if (SkinUtils.IsKnife((int)defIndex))
                {
                    var knives = await Db.GetKnifeSkinsAsync(steamId);
                    var knife = knives.FirstOrDefault(k => k.SteamID == steamId && k.Team == team);
                    if (knife == null) return;

                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        // Guard: weapon must still belong to the player we queried for.
                        if (!VerifyWeaponOwner(weaponRef, steamId, team)) return;
                        ApplyKnifeAttributes(weaponRef, knife);
                    });
                }
                else if (SkinUtils.IsWeapon((int)defIndex))
                {
                    var skins = await Db.GetWeaponSkinsAsync(steamId);
                    var skin = skins.FirstOrDefault(s =>
                        s.SteamID == steamId && s.Team == team && s.DefinitionIndex == defIndex);
                    if (skin == null) return;

                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        if (!VerifyWeaponOwner(weaponRef, steamId, team)) return;
                        // Also verify the weapon's definition index hasn't changed.
                        if (weaponRef.AttributeManager.Item.ItemDefinitionIndex != defIndex) return;
                        ApplyWeaponAttributes(weaponRef, skin);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OSTORA] GiveNamedItemPost DB error for {SteamID}", steamId);
            }
        });
    }

    // ================================================================
    //  WEAPON ATTRIBUTE APPLICATION (NO STICKER CACHE)
    // ================================================================
    private void ApplyWeaponAttributes(CBasePlayerWeapon weapon, WeaponSkinData skin)
    {
        var item = weapon.AttributeManager.Item;
        item.ItemDefinitionIndex = skin.DefinitionIndex;
        item.EntityQuality = (int)skin.Quality;
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", skin.Paintkit);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", skin.PaintkitSeed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", skin.PaintkitWear);
        item.AttributeList.SetOrAddAttribute("set item texture prefab", skin.Paintkit);
        item.AttributeList.SetOrAddAttribute("set item texture seed", skin.PaintkitSeed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", skin.PaintkitWear);

        if (skin.Quality == EconItemQuality.StatTrak)
        {
            var val = BitConverter.Int32BitsToSingle(skin.StattrakCount);
            item.AttributeList.SetOrAddAttribute("kill eater", val);
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater", val);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
        }

        if (skin.Nametag != null) item.CustomName = skin.Nametag;

        for (int i = 0; i < 5; i++)
        {
            var sticker = skin.GetSticker(i);
            if (sticker == null) continue;

            var stickerIdFloat = BitConverter.Int32BitsToSingle(sticker.Id);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} id", stickerIdFloat);
            item.AttributeList.SetOrAddAttribute($"sticker slot {i} id", stickerIdFloat);

            if (sticker.Schema != 1337)
            {
                var schemaFloat = BitConverter.Int32BitsToSingle(sticker.Schema);
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} schema", schemaFloat);
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset x", sticker.OffsetX);
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset y", sticker.OffsetY);
            }

            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} wear", sticker.Wear);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} scale", sticker.Scale);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} rotation", sticker.Rotation);

            item.AttributeList.SetOrAddAttribute($"sticker slot {i} wear", sticker.Wear);
            item.AttributeList.SetOrAddAttribute($"sticker slot {i} scale", sticker.Scale);
            item.AttributeList.SetOrAddAttribute($"sticker slot {i} rotation", sticker.Rotation);
        }

        var keychain = skin.Keychain0;
        if (keychain != null)
        {
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 id", BitConverter.Int32BitsToSingle(keychain.Id));
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset x", keychain.OffsetX);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset y", keychain.OffsetY);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 offset z", keychain.OffsetZ);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("keychain slot 0 seed", BitConverter.Int32BitsToSingle(keychain.Seed));
        }
    }

    // ================================================================
    //  KNIFE ATTRIBUTE APPLICATION
    // ================================================================
    private void ApplyKnifeAttributes(CBasePlayerWeapon weapon, KnifeSkinData knife)
    {
        var item = weapon.AttributeManager.Item;
        item.EntityQuality = (int)knife.Quality;
        if (knife.Nametag != null) item.CustomName = knife.Nametag;

        if (item.ItemDefinitionIndex != knife.DefinitionIndex)
            weapon.AcceptInputAsync("ChangeSubclass", knife.DefinitionIndex.ToString());

        item.ItemDefinitionIndex = knife.DefinitionIndex;

        if (knife.Quality == EconItemQuality.StatTrak)
        {
            var val = BitConverter.Int32BitsToSingle(knife.StattrakCount);
            item.AttributeList.SetOrAddAttribute("kill eater", val);
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater", val);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
        }
        else
        {
            item.AttributeList.Attributes.RemoveAll();
            item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        }

        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", knife.Paintkit);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", knife.PaintkitSeed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", knife.PaintkitWear);
        item.AttributeList.SetOrAddAttribute("set item texture prefab", knife.Paintkit);
        item.AttributeList.SetOrAddAttribute("set item texture seed", knife.PaintkitSeed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", knife.PaintkitWear);
    }

    // ================================================================
    //  GLOVE VISUAL APPLICATION (DATA-DRIVEN - NO CACHE)
    // ================================================================
    private void ApplyGloveVisualFromData(IPlayer player, GloveData glove, CCSPlayerInventory inv, Team team)
    {
        if (!player.IsAlive) return;

        var pawn = player.PlayerPawn!;
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.IsAlive) return;

            // Model swap trick to force glove refresh
            var model = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
            pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
            pawn.SetModel(model);

            // Reset and set Initialized immediately after model swap (key timing!)
            var econGloves = pawn.EconGloves;
            econGloves.Initialized = false;
            econGloves.Initialized = true;

            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (!player.IsAlive) return;
                ApplyGloveAttributesFromData(pawn, econGloves, glove, inv, team);
            });
        });
    }

    private void ApplyGloveAttributesFromData(CCSPlayerPawn pawn, CEconItemView econGloves, GloveData glove, CCSPlayerInventory inv, Team team)
    {
        // Read glove item from inventory loadout (ensures ItemID matches inventory)
        var itemInLoadout = inv.GetItemInLoadout(team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS);
        if (itemInLoadout != null)
        {
            econGloves.ItemDefinitionIndex = itemInLoadout.ItemDefinitionIndex;
            econGloves.AccountID = itemInLoadout.AccountID;
            econGloves.ItemID = itemInLoadout.ItemID;
            econGloves.ItemIDHigh = itemInLoadout.ItemIDHigh;
            econGloves.ItemIDLow = itemInLoadout.ItemIDLow;
            econGloves.InventoryPosition = itemInLoadout.InventoryPosition;
            econGloves.EntityLevel = itemInLoadout.EntityLevel;
            econGloves.EntityQuality = itemInLoadout.EntityQuality;
        }
        else
        {
            econGloves.ItemDefinitionIndex = glove.DefinitionIndex;
        }

        // Clear old attributes
        econGloves.AttributeList.Attributes.RemoveAll();
        econGloves.NetworkedDynamicAttributes.Attributes.RemoveAll();

        // Apply skin attributes
        econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", glove.Paintkit);
        econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", glove.PaintkitSeed);
        econGloves.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", glove.PaintkitWear);
        econGloves.AttributeList.SetOrAddAttribute("set item texture prefab", glove.Paintkit);
        econGloves.AttributeList.SetOrAddAttribute("set item texture seed", glove.PaintkitSeed);
        econGloves.AttributeList.SetOrAddAttribute("set item texture wear", glove.PaintkitWear);

        // Set bodygroup to show gloves
        pawn.AcceptInput("SetBodygroup", "default_gloves,1");
    }

    // ================================================================
    //  AGENT VISUAL APPLICATION (DATA-DRIVEN - NO CACHE)
    // ================================================================
    private void ApplyAgentVisualFromData(IPlayer player, int agentIndex)
    {
        if (!player.IsAlive) return;
        if (agentIndex == 0) return;

        var pawn = player.PlayerPawn!;
        var team = player.Controller.Team;
        var steamId = player.SteamID;

        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.IsAlive) return;

            var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

            // Capture default model per-player, per-team (so reset works later).
            var defaults = _defaultModels.GetOrAdd(steamId, _ => new ConcurrentDictionary<Team, string>());
            defaults.TryAdd(team, current);

            // Look up agent model by index using items_game definition
            var agentModelPath = GetAgentModelPath(agentIndex);

            if (agentModelPath != null)
            {
                // Swap trick for agent model refresh
                if (current != agentModelPath)
                {
                    pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
                    pawn.SetModel(current);
                }

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!player.IsAlive) return;
                    pawn.SetModel(agentModelPath);
                });
            }
            else
            {
                Logger.LogWarning("[OSTORA] Agent index {Index} not found in model lookup", agentIndex);
            }
        });
    }

    // ================================================================
    //  APPLY WEAPON ATTRIBUTES FROM DB DATA (NO CACHE)
    // ================================================================
    private void ApplyWeaponAttributesFromData(IPlayer player, List<WeaponSkinData> weapons, List<KnifeSkinData> knives, Team team)
    {
        if (!player.IsAlive) return;
        var pawn = player.PlayerPawn!;
        var playerSteamId = player.SteamID;

        foreach (var handle in pawn.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;

            // Belt-and-suspenders: the weapon must actually belong to this player
            // on this team before we touch it.
            if (!VerifyWeaponOwner(weapon, playerSteamId, team)) continue;

            var def = (int)weapon.AttributeManager.Item.ItemDefinitionIndex;

            if (SkinUtils.IsKnife(def))
            {
                var knife = knives.FirstOrDefault(k => k.SteamID == playerSteamId && k.Team == team);
                if (knife != null)
                {
                    ApplyKnifeAttributes(weapon, knife);
                }
            }
            else if (SkinUtils.IsWeapon(def))
            {
                var skin = weapons.FirstOrDefault(s =>
                    s.SteamID == playerSteamId && s.Team == team && s.DefinitionIndex == def);
                if (skin != null)
                {
                    ApplyWeaponAttributes(weapon, skin);
                }
            }
        }
    }

    // ================================================================
    //  AGENT MODEL LOOKUP
    // ================================================================
    private static string? GetAgentModelPath(int agentIndex)
    {
        return AgentModels.TryGetValue(agentIndex, out var path) ? path : null;
    }
}
