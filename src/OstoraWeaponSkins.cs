using System.Collections.Concurrent;

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

    // ── Per-player caches ───────────────────────────────────────
    private readonly ConcurrentDictionary<ulong, List<WeaponSkinData>> _weaponCache = new();
    private readonly ConcurrentDictionary<ulong, List<KnifeSkinData>> _knifeCache = new();
    private readonly ConcurrentDictionary<ulong, List<GloveData>> _gloveCache = new();
    private readonly ConcurrentDictionary<ulong, List<(Team Team, int AgentIndex)>> _agentCache = new();
    private readonly ConcurrentDictionary<ulong, List<MusicKitData>> _musicCache = new();
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<Team, string>> _defaultModels = new();

    // ── Inventory map (populated via SOCache hooks) ─────────────
    private readonly ConcurrentDictionary<ulong, CCSPlayerInventory> _inventories = new();

    // ── Agent models (parsed from items_game.txt) ───────────────
    private static readonly Dictionary<int, string> AgentModels = new();

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

        // SOCache hooks → track inventories
        Native.OnSOCacheSubscribed += (inv, soid) => _inventories[soid.SteamID] = inv;
        Native.OnSOCacheUnsubscribed += (inv, soid) => _inventories.TryRemove(soid.SteamID, out _);

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
    //  PLAYER SPAWN
    // ================================================================
    private HookResult OnPlayerSpawn(EventPlayerSpawn ev)
    {
        var player = ev.UserIdPlayer;
        if (player == null || !player.IsAlive) return HookResult.Continue;

        // If not cached yet, load from DB
        if (!_weaponCache.ContainsKey(player.SteamID))
            LoadAndApplyPlayer(player);
        else
            ApplyAllVisuals(player);

        // Re-apply after a short delay (engine may overwrite visuals)
        Core.Scheduler.DelayBySeconds(0.1f, () =>
        {
            if (!player.IsAlive) return;
            ApplyGloveVisual(player);
            ApplyAgentVisual(player);
        });

        return HookResult.Continue;
    }

    // ================================================================
    //  PLAYER TEAM CHANGE
    // ================================================================
    private HookResult OnPlayerTeam(EventPlayerTeam ev)
    {
        var player = ev.UserIdPlayer;
        if (player == null || ev.Disconnect) return HookResult.Continue;

        // Re-apply music kit for new team
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!_inventories.TryGetValue(player.SteamID, out var inv)) return;
            if (!_musicCache.TryGetValue(player.SteamID, out var music)) return;

            var musicForTeam = music.FirstOrDefault(m => m.Team == player.Controller.Team);
            if (musicForTeam != null)
                inv.UpdateMusicKit(musicForTeam.MusicID);
        });

        return HookResult.Continue;
    }

    // ================================================================
    //  LOAD FROM DB + APPLY
    // ================================================================
    private void LoadAndApplyPlayer(IPlayer player)
    {
        var steamId = player.SteamID;
        Task.Run(async () =>
        {
            try
            {
                var weapons = await Db.GetWeaponSkinsAsync(steamId);
                var knives = await Db.GetKnifeSkinsAsync(steamId);
                var gloves = await Db.GetGloveSkinsAsync(steamId);
                var agents = await Db.GetAgentsAsync(steamId);
                var music = await Db.GetMusicKitAsync(steamId);

                _weaponCache[steamId] = weapons;
                _knifeCache[steamId] = knives;
                _gloveCache[steamId] = gloves;
                _agentCache[steamId] = agents;
                _musicCache[steamId] = music;

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!_inventories.TryGetValue(steamId, out var inv)) return;

                    // Update inventory loadout for all items
                    foreach (var w in weapons) inv.UpdateWeaponSkin(w);
                    foreach (var k in knives) inv.UpdateKnifeSkin(k);
                    foreach (var g in gloves) inv.UpdateGloveSkin(g);
                    var musicForTeam = music.FirstOrDefault(m => m.Team == player.Controller.Team);
                    if (musicForTeam != null) inv.UpdateMusicKit(musicForTeam.MusicID);

                    // Apply visuals after inventory updates settle
                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        if (!player.IsAlive) return;
                        ApplyAllVisuals(player);
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
    //  RCON REFRESH
    // ================================================================
    private bool _isAutoRefresh = false;

    private void OnRefreshCommand(SwiftlyS2.Shared.Commands.ICommandContext context)
    {
        if (context.Args.Length < 1) return;
        if (!ulong.TryParse(context.Args[0], out var steamId))
        {
            Logger.LogWarning("[OSTORA] Invalid SteamID: {Arg}", context.Args[0]);
            return;
        }

        var isAuto = _isAutoRefresh;
        _isAutoRefresh = false;

        Logger.LogInformation("[OSTORA] Refresh requested for {SteamID} (Auto: {IsAuto})", steamId, isAuto);

        Task.Run(async () =>
        {
            try
            {
                var weapons = await Db.GetWeaponSkinsAsync(steamId);
                var knives = await Db.GetKnifeSkinsAsync(steamId);
                var gloves = await Db.GetGloveSkinsAsync(steamId);
                var agents = await Db.GetAgentsAsync(steamId);
                var music = await Db.GetMusicKitAsync(steamId);

                _weaponCache[steamId] = weapons;
                _knifeCache[steamId] = knives;
                _gloveCache[steamId] = gloves;
                _agentCache[steamId] = agents;
                _musicCache[steamId] = music;

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!_inventories.TryGetValue(steamId, out var inv))
                    {
                        Logger.LogWarning("[OSTORA] No inventory for {SteamID}", steamId);
                        return;
                    }

                    // Find the player
                    IPlayer? player = null;
                    foreach (var p in Core.PlayerManager.GetAllPlayers())
                    {
                        if (p.SteamID == steamId) { player = p; break; }
                    }
                    if (player == null)
                    {
                        Logger.LogWarning("[OSTORA] Player {SteamID} not found online", steamId);
                        return;
                    }

                    // Update inventory loadout
                    foreach (var w in weapons) inv.UpdateWeaponSkin(w);
                    foreach (var k in knives) inv.UpdateKnifeSkin(k);

                    // Gloves: update inventory for all teams
                    foreach (var g in gloves) inv.UpdateGloveSkin(g);

                    // Music kit: filter by team
                    var musicForTeam = music.FirstOrDefault(m => m.Team == player.Controller.Team);
                    if (musicForTeam != null) inv.UpdateMusicKit(musicForTeam.MusicID);

                    // Apply visuals after inventory updates settle (2 ticks)
                    Core.Scheduler.NextWorldUpdate(() =>
                    {
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (!player.IsAlive) return;

                            // Regive all held weapons
                            RegivePlayerWeapons(player);

                            // Apply glove visual
                            ApplyGloveVisual(player);

                            // Apply agent visual
                            ApplyAgentVisual(player);

                            Logger.LogInformation("[OSTORA] Refresh complete for {SteamID}", steamId);

                            // Auto-send the command again after 0.5s to fix glove textures (only if not already an auto-refresh)
                            if (!isAuto)
                            {
                                Core.Scheduler.DelayBySeconds(0.5f, () =>
                                {
                                    _isAutoRefresh = true;
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
    //  GIVE NAMED ITEM POST → apply skin attributes on weapon pickup
    // ================================================================
    private void OnGiveNamedItemPost(CCSPlayer_ItemServices services, CBasePlayerWeapon weapon)
    {
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

            var steamId = controller.SteamID;
            var team = controller.Team;
            var defIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;

            if (SkinUtils.IsKnife((int)defIndex))
            {
                if (_knifeCache.TryGetValue(steamId, out var knives))
                {
                    var knife = knives.FirstOrDefault(k => k.Team == team);
                    if (knife != null)
                        ApplyKnifeAttributes(weapon, knife);
                }
            }
            else if (SkinUtils.IsWeapon((int)defIndex))
            {
                if (_weaponCache.TryGetValue(steamId, out var skins))
                {
                    var skin = skins.FirstOrDefault(s => s.Team == team && s.DefinitionIndex == defIndex);
                    if (skin != null)
                        ApplyWeaponAttributes(weapon, skin);
                }
            }
        }
        catch (Exception e) { Logger.LogError(e, "[OSTORA] GiveNamedItemPost error"); }
    }

    // ================================================================
    //  APPLY ALL VISUALS
    // ================================================================
    private void ApplyAllVisuals(IPlayer player)
    {
        if (!player.IsAlive) return;

        // Weapons: apply attributes to all currently held weapons
        foreach (var handle in player.PlayerPawn!.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            var def = weapon.AttributeManager.Item.ItemDefinitionIndex;

            if (SkinUtils.IsKnife((int)def))
            {
                if (_knifeCache.TryGetValue(player.SteamID, out var knives))
                {
                    var knife = knives.FirstOrDefault(k => k.Team == player.Controller.Team);
                    if (knife != null) ApplyKnifeAttributes(weapon, knife);
                }
            }
            else if (SkinUtils.IsWeapon((int)def))
            {
                if (_weaponCache.TryGetValue(player.SteamID, out var skins))
                {
                    var skin = skins.FirstOrDefault(s => s.Team == player.Controller.Team && s.DefinitionIndex == def);
                    if (skin != null) ApplyWeaponAttributes(weapon, skin);
                }
            }
        }

        // Gloves
        ApplyGloveVisual(player);

        // Agents
        ApplyAgentVisual(player);
    }

    // ================================================================
    //  WEAPON ATTRIBUTE APPLICATION
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

        for (int i = 0; i < 6; i++)
        {
            var sticker = skin.GetSticker(i);
            if (sticker == null) continue;
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} id", BitConverter.Int32BitsToSingle(sticker.Id));
            if (sticker.Schema != 1337)
            {
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} schema", BitConverter.Int32BitsToSingle(sticker.Schema));
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset x", sticker.OffsetX);
                item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} offset y", sticker.OffsetY);
            }
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} wear", sticker.Wear);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} scale", sticker.Scale);
            item.NetworkedDynamicAttributes.SetOrAddAttribute($"sticker slot {i} rotation", sticker.Rotation);
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
    //  GLOVE VISUAL APPLICATION
    // ================================================================
    private void ApplyGloveVisual(IPlayer player)
    {
        if (!player.IsAlive) return;
        if (!_gloveCache.TryGetValue(player.SteamID, out var gloves)) return;

        var glove = gloves.FirstOrDefault(g => g.Team == player.Controller.Team);
        if (glove == null) return;

        // Need inventory to read loadout item
        if (!_inventories.TryGetValue(player.SteamID, out var inv)) return;

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
                ApplyGloveAttributes(pawn, econGloves, glove, inv, player.Controller.Team);
            });
        });
    }

    private void ApplyGloveAttributes(CCSPlayerPawn pawn, CEconItemView econGloves, GloveData glove, CCSPlayerInventory inv, Team team)
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
    //  AGENT VISUAL APPLICATION
    // ================================================================
    private void ApplyAgentVisual(IPlayer player)
    {
        if (!player.IsAlive) return;
        if (!_agentCache.TryGetValue(player.SteamID, out var agents)) return;

        var pawn = player.PlayerPawn!;
        var team = player.Controller.Team;

        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.IsAlive) return;

            var current = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

            // Capture default model
            var defaults = _defaultModels.GetOrAdd(player.SteamID, _ => new ConcurrentDictionary<Team, string>());
            defaults.TryAdd(team, current);

            var agentEntry = agents.FirstOrDefault(a => a.Team == team);

            if (agentEntry.AgentIndex != 0)
            {
                // Look up agent model by index using items_game definition
                var agentModelPath = GetAgentModelPath(agentEntry.AgentIndex);

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
                    Logger.LogWarning("[OSTORA] Agent index {Index} not found in model lookup", agentEntry.AgentIndex);
                }
            }
            else if (defaults.TryGetValue(team, out var defaultModel))
            {
                Core.Scheduler.NextWorldUpdate(() =>
                {
                    if (!player.IsAlive) return;
                    pawn.SetModel(defaultModel);
                });
            }
        });
    }

    // ================================================================
    //  REGIVE WEAPONS (for refresh)
    // ================================================================
    private void RegivePlayerWeapons(IPlayer player)
    {
        if (!player.IsAlive) return;
        var pawn = player.PlayerPawn!;
        var team = player.Controller.Team;

        foreach (var handle in pawn.WeaponServices!.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;
            var def = (int)weapon.AttributeManager.Item.ItemDefinitionIndex;

            if (SkinUtils.IsKnife(def))
            {
                if (_knifeCache.TryGetValue(player.SteamID, out var knives))
                {
                    var knife = knives.FirstOrDefault(k => k.Team == team);
                    if (knife != null)
                    {
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            pawn.WeaponServices!.RemoveWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
                            pawn.ItemServices!.GiveItem("weapon_knife");
                            pawn.WeaponServices!.SelectWeaponBySlot(gear_slot_t.GEAR_SLOT_KNIFE);
                        });
                    }
                }
            }
            else if (SkinUtils.IsWeapon(def))
            {
                if (_weaponCache.TryGetValue(player.SteamID, out var skins))
                {
                    var skin = skins.FirstOrDefault(s => s.Team == team && s.DefinitionIndex == (ushort)def);
                    if (skin != null)
                    {
                        var defIdx = skin.DefinitionIndex;
                        var weaponRef = weapon;
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (defIdx == Core.Helpers.GetDefinitionIndexByClassname("weapon_taser"))
                            {
                                RegiveWeaponTaser(player, weaponRef);
                            }
                            else
                            {
                                var name = Core.Helpers.GetClassnameByDefinitionIndex(defIdx)!;
                                var clip1 = weaponRef.Clip1;
                                var reservedAmmo = weaponRef.ReserveAmmo[0];
                                pawn.WeaponServices!.RemoveWeapon(weaponRef);
                                var newWeapon = pawn.ItemServices!.GiveItem<CBasePlayerWeapon>(name);
                                newWeapon.Clip1 = clip1;
                                newWeapon.ReserveAmmo[0] = reservedAmmo;
                            }
                        });
                    }
                }
            }
        }
    }

    private void RegiveWeaponTaser(IPlayer player, CBasePlayerWeapon weapon)
    {
        var pawn = player.PlayerPawn!;
        var taser = weapon.As<CWeaponTaser>();
        var clip1 = taser.Clip1;
        var reservedAmmo = taser.ReserveAmmo[0];
        var fireTime = taser.FireTime.Value;
        var lastAttackTick = taser.LastAttackTick;
        pawn.WeaponServices!.RemoveWeapon(weapon);
        var newWeapon = pawn.ItemServices!.GiveItem<CWeaponTaser>("weapon_taser");
        newWeapon.Clip1 = clip1;
        newWeapon.ReserveAmmo[0] = reservedAmmo;
        newWeapon.FireTime.Value = fireTime;
        newWeapon.LastAttackTick = lastAttackTick;
    }

    // ================================================================
    //  AGENT MODEL LOOKUP
    // ================================================================
    private static string? GetAgentModelPath(int agentIndex)
    {
        return AgentModels.TryGetValue(agentIndex, out var path) ? path : null;
    }
}
