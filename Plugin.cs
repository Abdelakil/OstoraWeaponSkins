using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Database;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace OstoraWeaponSkins;

[PluginMetadata(Id = "ostoraweaponskins", Version = "1.0", Name = "Ostora Weapon Skins", Author = "Ostora")]
public sealed partial class OstoraWeaponSkins : BasePlugin
{
    public new static ISwiftlyCore Core { get; private set; } = null!;

    private static PluginConfig _config = new();
    internal static Database? Database;

    internal static readonly Dictionary<string, string> WeaponList = new()
    {
        { "weapon_deagle", "Desert Eagle" },
        { "weapon_elite", "Dual Berettas" },
        { "weapon_fiveseven", "Five-SeveN" },
        { "weapon_glock", "Glock-18" },
        { "weapon_ak47", "AK-47" },
        { "weapon_aug", "AUG" },
        { "weapon_awp", "AWP" },
        { "weapon_famas", "FAMAS" },
        { "weapon_g3sg1", "G3SG1" },
        { "weapon_galilar", "Galil AR" },
        { "weapon_m249", "M249" },
        { "weapon_m4a1", "M4A4" },
        { "weapon_mac10", "MAC-10" },
        { "weapon_p90", "P90" },
        { "weapon_mp5sd", "MP5-SD" },
        { "weapon_ump45", "UMP-45" },
        { "weapon_xm1014", "XM1014" },
        { "weapon_bizon", "PP-Bizon" },
        { "weapon_mag7", "MAG-7" },
        { "weapon_negev", "Negev" },
        { "weapon_sawedoff", "Sawed-Off" },
        { "weapon_tec9", "Tec-9" },
        { "weapon_taser", "Zeus x27" },
        { "weapon_hkp2000", "P2000" },
        { "weapon_mp7", "MP7" },
        { "weapon_mp9", "MP9" },
        { "weapon_nova", "Nova" },
        { "weapon_p250", "P250" },
        { "weapon_scar20", "SCAR-20" },
        { "weapon_sg556", "SG 553" },
        { "weapon_ssg08", "SSG 08" },
        { "weapon_m4a1_silencer", "M4A1-S" },
        { "weapon_usp_silencer", "USP-S" },
        { "weapon_cz75a", "CZ75-Auto" },
        { "weapon_revolver", "R8 Revolver" },
        { "weapon_knife", "Default Knife" },
        { "weapon_knife_m9_bayonet", "M9 Bayonet" },
        { "weapon_knife_karambit", "Karambit" },
        { "weapon_bayonet", "Bayonet" },
        { "weapon_knife_survival_bowie", "Bowie Knife" },
        { "weapon_knife_butterfly", "Butterfly Knife" },
        { "weapon_knife_falchion", "Falchion Knife" },
        { "weapon_knife_flip", "Flip Knife" },
        { "weapon_knife_gut", "Gut Knife" },
        { "weapon_knife_tactical", "Huntsman Knife" },
        { "weapon_knife_push", "Shadow Daggers" },
        { "weapon_knife_gypsy_jackknife", "Navaja Knife" },
        { "weapon_knife_stiletto", "Stiletto Knife" },
        { "weapon_knife_widowmaker", "Talon Knife" },
        { "weapon_knife_ursus", "Ursus Knife" },
        { "weapon_knife_css", "Classic Knife" },
        { "weapon_knife_cord", "Paracord Knife" },
        { "weapon_knife_canis", "Survival Knife" },
        { "weapon_knife_outdoor", "Nomad Knife" },
        { "weapon_knife_skeleton", "Skeleton Knife" },
        { "weapon_knife_kukri", "Kukri Knife" }
    };

    internal static Dictionary<int, string> WeaponDefindex { get; } = new()
    {
        { 1, "weapon_deagle" }, { 2, "weapon_elite" }, { 3, "weapon_fiveseven" },
        { 4, "weapon_glock" }, { 7, "weapon_ak47" }, { 8, "weapon_aug" },
        { 9, "weapon_awp" }, { 10, "weapon_famas" }, { 11, "weapon_g3sg1" },
        { 13, "weapon_galilar" }, { 14, "weapon_m249" }, { 16, "weapon_m4a1" },
        { 17, "weapon_mac10" }, { 19, "weapon_p90" }, { 23, "weapon_mp5sd" },
        { 24, "weapon_ump45" }, { 25, "weapon_xm1014" }, { 26, "weapon_bizon" },
        { 27, "weapon_mag7" }, { 28, "weapon_negev" }, { 29, "weapon_sawedoff" },
        { 30, "weapon_tec9" }, { 31, "weapon_taser" }, { 32, "weapon_hkp2000" },
        { 33, "weapon_mp7" }, { 34, "weapon_mp9" }, { 35, "weapon_nova" },
        { 36, "weapon_p250" }, { 38, "weapon_scar20" }, { 39, "weapon_sg556" },
        { 40, "weapon_ssg08" }, { 60, "weapon_m4a1_silencer" }, { 61, "weapon_usp_silencer" },
        { 63, "weapon_cz75a" }, { 64, "weapon_revolver" },
        { 500, "weapon_bayonet" }, { 503, "weapon_knife_css" },
        { 505, "weapon_knife_flip" }, { 506, "weapon_knife_gut" },
        { 507, "weapon_knife_karambit" }, { 508, "weapon_knife_m9_bayonet" },
        { 509, "weapon_knife_tactical" }, { 512, "weapon_knife_falchion" },
        { 514, "weapon_knife_survival_bowie" }, { 515, "weapon_knife_butterfly" },
        { 516, "weapon_knife_push" }, { 517, "weapon_knife_cord" },
        { 518, "weapon_knife_canis" }, { 519, "weapon_knife_ursus" },
        { 520, "weapon_knife_gypsy_jackknife" }, { 521, "weapon_knife_outdoor" },
        { 522, "weapon_knife_stiletto" }, { 523, "weapon_knife_widowmaker" },
        { 525, "weapon_knife_skeleton" }, { 526, "weapon_knife_kukri" }
    };

    internal static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, string>> GPlayersKnife = new();
    internal static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ushort>> GPlayersGlove = new();
    internal static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ushort>> GPlayersMusic = new();
    internal static readonly ConcurrentDictionary<int, (string? CT, string? T)> GPlayersAgent = new();
    internal static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ConcurrentDictionary<int, WeaponInfo>>> GPlayerWeaponsInfo = new();
    internal static List<JObject> SkinsList = [];
    internal static List<JObject> GlovesList = [];
    internal static List<JObject> AgentsList = [];
    internal static List<JObject> MusicList = [];

    // Pre-built lookup: agent_index (extracted from image URL) → JObject agent entry
    internal static readonly Dictionary<int, JObject> AgentIndexLookup = [];
    internal static PluginConfig GetConfig() => _config;

    internal static void LogDebug(string message)
    {
        if (_config.DebugLogging)
            Console.WriteLine(message);
    }

    internal static string Localize(string key, params object[] args)
    {
        try
        {
            var prefix = Core.Localizer["prefix"];
            var message = args.Length > 0 ? Core.Localizer[key, args] : Core.Localizer[key];
            return prefix + message;
        }
        catch
        {
            return $"[OSTORA] {key}";
        }
    }

    private static bool _gBCommandsAllowed = true;
    private static readonly Dictionary<int, DateTime> CommandsCooldown = new();
    private static ulong _nextItemId = MinimumCustomItemId;
    private const ulong MinimumCustomItemId = 65578;

    public OstoraWeaponSkins(ISwiftlyCore core) : base(core)
    {
    }

    public override void Load(bool hotReload)
    {
        Core = base.Core;

        var configDir = Path.Combine(Core.PluginPath, "..", "..", "configs", "plugins", "ostoraweaponskins");
        Directory.CreateDirectory(configDir);
        var configPath = Path.Combine(configDir, "config.json");
        Core.Configuration.InitializeJsonWithModel<PluginConfig>(configPath, "OstoraWeaponSkins");
        _config = LoadConfigFromFile(configPath, "OstoraWeaponSkins");

        Database = new Database(_config.DatabaseConnection);

        LoadDataFiles();
        _ = CheckDatabaseTables();

        HookEvents();
        RegisterCommands();
    }

    public override void Unload()
    {
    }

    private void HookEvents()
    {
        Core.Event.OnClientPutInServer += (IOnClientPutInServerEvent e) =>
        {
            var player = Core.PlayerManager.GetPlayer(e.PlayerId);
            if (player != null) OnClientPutInServer(player);
        };
        Core.Event.OnClientDisconnected += (IOnClientDisconnectedEvent e) =>
        {
            var player = Core.PlayerManager.GetPlayer(e.PlayerId);
            if (player != null) OnClientDisconnected(player);
        };
        Core.Event.OnEntitySpawned += (IOnEntitySpawnedEvent e) =>
        {
            if (e.Entity != null) OnEntitySpawned(e.Entity);
        };

        Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);
        Core.GameEvent.HookPost<EventRoundStart>(OnRoundStart);
        Core.GameEvent.HookPost<EventRoundEnd>(OnRoundEnd);
        Core.GameEvent.HookPost<EventRoundMvp>(OnRoundMvp);
        Core.GameEvent.HookPost<EventPlayerDeath>(OnPlayerDeath);
    }

    private void OnClientPutInServer(IPlayer player)
    {
        if (player.IsFakeClient || Database == null)
            return;

        LogDebug($"[OstoraWeaponSkins] Player connected: {player.Name} (SteamID: {player.SteamID})");

        _ = Task.Run(async () =>
        {
            try
            {
                LogDebug($"[OstoraWeaponSkins] Loading player data from DB for {player.Name}...");
                await Database.LoadPlayerData(player);
                LogDebug($"[OstoraWeaponSkins] Player data loaded for {player.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OstoraWeaponSkins] Error in OnClientPutInServer: {ex.Message}");
            }
        });
    }

    private void OnClientDisconnected(IPlayer player)
    {
        if (player.IsFakeClient) return;

        Task.Run(async () =>
        {
            try
            {
                await SyncStatTrakToDatabase(player);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OstoraWeaponSkins] Error in OnClientDisconnected: {ex.Message}");
            }
        });

        if (_config.SkinEnabled)
            GPlayerWeaponsInfo.TryRemove(player.Slot, out _);
        if (_config.KnifeEnabled)
            GPlayersKnife.TryRemove(player.Slot, out _);
        if (_config.GloveEnabled)
            GPlayersGlove.TryRemove(player.Slot, out _);
        if (_config.AgentEnabled)
            GPlayersAgent.TryRemove(player.Slot, out _);
        if (_config.MusicEnabled)
            GPlayersMusic.TryRemove(player.Slot, out _);

        CommandsCooldown.Remove(player.Slot);
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (!entity.DesignerName.Contains("weapon")) return;

        LogDebug($"[OstoraWeaponSkins] Entity spawned: {entity.DesignerName}");

        Core.Scheduler.NextWorldUpdate(() =>
        {
            try
            {
                LogDebug($"[OstoraWeaponSkins] NextWorldUpdate: checking {entity.DesignerName}");

                if (entity is not CBaseEntity baseEntity || !baseEntity.IsValid)
                {
                    LogDebug($"[OstoraWeaponSkins] NextWorldUpdate: {entity.DesignerName} is not CBaseEntity or invalid");
                    return;
                }

                var ownerHandle = baseEntity.OwnerEntity;
                LogDebug($"[OstoraWeaponSkins] NextWorldUpdate: {entity.DesignerName} ownerHandle valid={ownerHandle.IsValid}");
                if (!ownerHandle.IsValid) return;

                // OwnerEntity is the pawn, not the controller. Use GetPlayerFromPawn.
                if (ownerHandle.Value is CBasePlayerPawn pawn && pawn.IsValid)
                {
                    var p = Core.PlayerManager.GetPlayerFromPawn(pawn);
                    if (p != null)
                    {
                        LogDebug($"[OstoraWeaponSkins] NextWorldUpdate: Applying skin to {p.Name}'s {entity.DesignerName}");
                        GivePlayerWeaponSkin(p, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OstoraWeaponSkins] Error in OnEntitySpawned: {ex.Message}");
            }
        });
    }

    public HookResult OnPlayerSpawn(EventPlayerSpawn @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || player.IsFakeClient)
            return HookResult.Continue;

        LogDebug($"[OstoraWeaponSkins] Player spawned: {player.Name}, applying skins...");

        if (!_config.KnifeEnabled && !_config.GloveEnabled)
            return HookResult.Continue;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        GivePlayerMusicKit(player);
        GivePlayerAgent(player);
        Core.Scheduler.NextTick(() =>
        {
            GivePlayerGloves(player);
        });

        return HookResult.Continue;
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        var attacker = @event.AttackerPlayer;
        var victim = @event.UserIdPlayer;

        if (attacker == null || victim == null || victim == attacker)
            return HookResult.Continue;

        var controller = attacker.Controller;
        if (controller == null || !controller.IsValid)
            return HookResult.Continue;

        var pawn = attacker.PlayerPawn;
        if (pawn == null || pawn.WeaponServices == null)
            return HookResult.Continue;

        var activeWeapon = pawn.WeaponServices.ActiveWeapon.Value;
        if (activeWeapon == null) return HookResult.Continue;

        int weaponDefIndex = activeWeapon.AttributeManager.Item.ItemDefinitionIndex;

        if (!HasChangedPaint(attacker, weaponDefIndex, out var weaponInfo) || weaponInfo == null)
            return HookResult.Continue;

        if (!weaponInfo.StatTrak) return HookResult.Continue;

        weaponInfo.StatTrakCount += 1;

        return HookResult.Continue;
    }

    public HookResult OnRoundStart(EventRoundStart @event)
    {
        _gBCommandsAllowed = true;
        return HookResult.Continue;
    }

    public HookResult OnRoundEnd(EventRoundEnd @event)
    {
        _gBCommandsAllowed = false;
        return HookResult.Continue;
    }

    public HookResult OnRoundMvp(EventRoundMvp @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || player.IsFakeClient)
            return HookResult.Continue;

        if (!GPlayersMusic.TryGetValue(player.Slot, out var musicInfo))
            return HookResult.Continue;

        var controller = player.Controller;
        if (controller == null) return HookResult.Continue;

        int team = controller.TeamNum;
        if (!musicInfo.TryGetValue(team, out var musicId) || musicId == 0)
            return HookResult.Continue;

        @event.MusickItID = musicId;
        @event.NoMusic = 0;

        return HookResult.Continue;
    }

    private void RegisterCommands()
    {
        Core.Command.RegisterCommand("ws_refreshskins", OnCommandSkinRefresh, true);
    }

    private void OnCommandRefresh(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || player.IsFakeClient || !_config.SkinEnabled || !_gBCommandsAllowed)
            return;

        if (CommandsCooldown.TryGetValue(player.Slot, out var cooldownEndTime) &&
            DateTime.UtcNow < cooldownEndTime)
        {
            player.SendChat(Localize("cooldown"));
            return;
        }

        CommandsCooldown[player.Slot] = DateTime.UtcNow.AddSeconds(_config.CmdRefreshCooldownSeconds);

        _ = Task.Run(async () =>
        {
            if (Database != null)
                await Database.LoadPlayerData(player);
        });

        GivePlayerGloves(player);
        RefreshWeapons(player);
        GivePlayerAgent(player);
        GivePlayerMusicKit(player);

        player.SendChat(Localize("refresh_done"));
    }

    private void OnCommandWS(ICommandContext context)
    {
        var player = context.Sender;
        if (player == null || player.IsFakeClient || !_config.SkinEnabled)
            return;

        player.SendChat(Localize("website"));
        player.SendChat(Localize("refresh_hint"));
    }

    private void OnCommandSkinRefresh(ICommandContext context)
    {
        LogDebug($"[OstoraWeaponSkins] ws_refreshskins called with args: {string.Join(", ", context.Args)}");

        if (context.IsSentByPlayer)
            return;

        var args = context.Args.Length > 0 ? context.Args[0] : null;

        if (string.IsNullOrEmpty(args))
        {
            LogDebug("[OstoraWeaponSkins] Usage: ws_refreshskins <steamid64|all>");
            return;
        }

        List<IPlayer> targetPlayers = [];

        if (args.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            targetPlayers = Core.PlayerManager.GetAllPlayers()
                .Where(p => !p.IsFakeClient)
                .ToList();

            if (targetPlayers.Count == 0)
            {
                LogDebug("[OstoraWeaponSkins] No players connected to refresh.");
                return;
            }

            LogDebug($"[OstoraWeaponSkins] Refreshing skins for {targetPlayers.Count} players...");
        }
        else
        {
            if (!ulong.TryParse(args, out var steamId))
            {
                LogDebug("[OstoraWeaponSkins] Invalid SteamID64 format.");
                return;
            }

            var found = Core.PlayerManager.GetAllPlayers()
                .FirstOrDefault(p => !p.IsFakeClient && p.SteamID == steamId);

            if (found == null)
            {
                LogDebug($"[OstoraWeaponSkins] Player with SteamID64 '{args}' not found.");
                return;
            }

            targetPlayers.Add(found);
            LogDebug($"[OstoraWeaponSkins] Refreshing skins for {found.Name}...");
        }

        foreach (var target in targetPlayers)
        {
            try
            {
                LogDebug($"[OstoraWeaponSkins] Loading player data from database for {target.Name}...");
                _ = Task.Run(async () =>
                {
                    if (Database != null)
                        await Database.LoadPlayerData(target);
                });

                LogDebug($"[OstoraWeaponSkins] Data loaded. Applying skins...");
                LogDebug($"[OstoraWeaponSkins] Applying gloves for {target.Name}...");
                GivePlayerGloves(target);
                LogDebug($"[OstoraWeaponSkins] Applying weapons for {target.Name}...");
                RefreshWeapons(target);
                LogDebug($"[OstoraWeaponSkins] Applying agents for {target.Name}...");
                GivePlayerAgent(target);
                LogDebug($"[OstoraWeaponSkins] Applying music for {target.Name}...");
                GivePlayerMusicKit(target);

                LogDebug($"[OstoraWeaponSkins] Player {target.Name} slot={target.Slot}:");
                LogDebug($"[OstoraWeaponSkins]   Skins: {(GPlayerWeaponsInfo.TryGetValue(target.Slot, out var wi) ? wi.Count : 0)} team entries");
                LogDebug($"[OstoraWeaponSkins]   Knife: {(GPlayersKnife.TryGetValue(target.Slot, out var k) ? $"{k.Count} teams" : "none")}");
                LogDebug($"[OstoraWeaponSkins]   Gloves: {(GPlayersGlove.TryGetValue(target.Slot, out var g) ? $"{g.Count} teams" : "none")}");
                LogDebug($"[OstoraWeaponSkins]   Agents: {(GPlayersAgent.TryGetValue(target.Slot, out var a) ? $"CT={a.CT}, T={a.T}" : "none")}");
                LogDebug($"[OstoraWeaponSkins]   Music: {(GPlayersMusic.TryGetValue(target.Slot, out var m) ? $"{m.Count} teams" : "none")}");

                LogDebug($"[OstoraWeaponSkins] Skins refreshed for {target.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OstoraWeaponSkins] Error refreshing skins for {target.Name}: {ex.Message}");
            }
        }

        LogDebug("[OstoraWeaponSkins] Refresh process completed.");
    }

    #region Database Synchronization

    internal async Task SyncWeaponPaintsToDatabase(IPlayer player)
    {
        if (Database == null) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId) || !GPlayerWeaponsInfo.TryGetValue(player.Slot, out var teamWeaponInfos))
            return;

        try
        {
            await using var connection = await Database.GetConnectionAsync();

            foreach (var (teamId, weaponsInfo) in teamWeaponInfos)
            {
                foreach (var (weaponDefIndex, weaponInfo) in weaponsInfo)
                {
                    const string queryCheck = "SELECT COUNT(*) FROM `wp_player_skins` WHERE `steamid` = @steamid AND `weapon_defindex` = @weaponDefIndex AND `weapon_team` = @weaponTeam";
                    var existingCount = await connection.ExecuteScalarAsync<int>(
                        queryCheck,
                        new { steamid = steamId, weaponDefIndex, weaponTeam = (int)teamId });

                    if (existingCount > 0)
                    {
                        const string queryUpdate = "UPDATE `wp_player_skins` SET `weapon_paint_id` = @paintId, `weapon_wear` = @wear, `weapon_seed` = @seed WHERE `steamid` = @steamid AND `weapon_defindex` = @weaponDefIndex AND `weapon_team` = @weaponTeam";
                        await connection.ExecuteAsync(queryUpdate, new { steamid = steamId, weaponDefIndex, weaponTeam = (int)teamId, paintId = weaponInfo.Paint, wear = weaponInfo.Wear, seed = weaponInfo.Seed });
                    }
                    else
                    {
                        const string queryInsert = "INSERT INTO `wp_player_skins` (`steamid`, `weapon_defindex`, `weapon_team`, `weapon_paint_id`, `weapon_wear`, `weapon_seed`) VALUES (@steamid, @weaponDefIndex, @weaponTeam, @paintId, @wear, @seed)";
                        await connection.ExecuteAsync(queryInsert, new { steamid = steamId, weaponDefIndex, weaponTeam = (int)teamId, paintId = weaponInfo.Paint, wear = weaponInfo.Wear, seed = weaponInfo.Seed });
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing weapon paints: {e.Message}");
        }
    }

    internal async Task SyncKnifeToDatabase(IPlayer player, string knife, int[] teams)
    {
        if (Database == null || !_config.KnifeEnabled) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId) || string.IsNullOrEmpty(knife) || teams.Length == 0) return;

        const string query = "INSERT INTO `wp_player_knife` (`steamid`, `weapon_team`, `knife`) VALUES(@steamid, @team, @newKnife) ON DUPLICATE KEY UPDATE `knife` = @newKnife";

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            foreach (var team in teams)
            {
                await connection.ExecuteAsync(query, new { steamid = steamId, team, newKnife = knife });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing knife: {e.Message}");
        }
    }

    internal async Task SyncGloveToDatabase(IPlayer player, ushort gloveDefIndex, int[] teams)
    {
        if (Database == null || !_config.GloveEnabled) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId) || teams.Length == 0) return;

        const string query = "INSERT INTO `wp_player_gloves` (`steamid`, `weapon_team`, `weapon_defindex`) VALUES(@steamid, @team, @gloveDefIndex) ON DUPLICATE KEY UPDATE `weapon_defindex` = @gloveDefIndex";

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            foreach (var team in teams)
            {
                await connection.ExecuteAsync(query, new { steamid = steamId, team = (int)team, gloveDefIndex });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing glove: {e.Message}");
        }
    }

    internal async Task SyncAgentToDatabase(IPlayer player)
    {
        if (Database == null || !_config.AgentEnabled) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId)) return;

        if (!GPlayersAgent.TryGetValue(player.Slot, out var agent)) return;

        const string query = "INSERT INTO `wp_player_agents` (`steamid`, `agent_ct`, `agent_t`) VALUES(@steamid, @agent_ct, @agent_t) ON DUPLICATE KEY UPDATE `agent_ct` = @agent_ct, `agent_t` = @agent_t";

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            await connection.ExecuteAsync(query, new { steamid = steamId, agent_ct = agent.CT, agent_t = agent.T });
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing agent: {e.Message}");
        }
    }

    internal async Task SyncMusicToDatabase(IPlayer player, ushort music, int[] teams)
    {
        if (Database == null || !_config.MusicEnabled) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId)) return;

        const string query = "INSERT INTO `wp_player_music` (`steamid`, `weapon_team`, `music_id`) VALUES(@steamid, @team, @newMusic) ON DUPLICATE KEY UPDATE `music_id` = @newMusic";

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            foreach (var team in teams)
            {
                await connection.ExecuteAsync(query, new { steamid = steamId, team, newMusic = music });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing music: {e.Message}");
        }
    }

    private async Task SyncStatTrakToDatabase(IPlayer player)
    {
        if (Database == null) return;

        string? steamId = player.SteamID.ToString();
        if (string.IsNullOrEmpty(steamId)) return;

        if (!GPlayerWeaponsInfo.TryGetValue(player.Slot, out var teamWeaponsInfo)) return;

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var (teamKey, weaponInfos) in teamWeaponsInfo)
            {
                int weaponTeam = (int)teamKey;

                var statTrakWeapons = weaponInfos
                    .Where(w => w.Value.StatTrak)
                    .ToDictionary(w => w.Key, w => (w.Value.StatTrak, w.Value.StatTrakCount));

                if (statTrakWeapons.Count == 0) continue;

                foreach (var (defindex, (statTrak, statTrakCount)) in statTrakWeapons)
                {
                    const string query = @"UPDATE `wp_player_skins` SET `weapon_stattrak` = @StatTrak, `weapon_stattrak_count` = @StatTrakCount WHERE `steamid` = @steamid AND `weapon_defindex` = @weaponDefIndex AND `weapon_team` = @weaponTeam";

                    await connection.ExecuteAsync(query, new
                    {
                        steamid = steamId,
                        weaponDefIndex = defindex,
                        StatTrak = statTrak,
                        StatTrakCount = statTrakCount,
                        weaponTeam
                    }, transaction);
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error syncing stattrak: {e.Message}");
        }
    }

    #endregion

    #region Utility

    private void LoadDataFiles()
    {
        LogDebug($"[OstoraWeaponSkins] Loading data files from {Core.PluginPath}/data/...");
        LoadSkinsFromFile(Core.PluginPath + $"/data/skins_{_config.SkinsLanguage}.json");
        LoadGlovesFromFile(Core.PluginPath + $"/data/gloves_{_config.SkinsLanguage}.json");
        LoadAgentsFromFile(Core.PluginPath + $"/data/agents_{_config.SkinsLanguage}.json");
        LoadMusicFromFile(Core.PluginPath + $"/data/music_{_config.SkinsLanguage}.json");
        LogDebug($"[OstoraWeaponSkins] Loaded {SkinsList.Count} skins, {GlovesList.Count} gloves, {AgentsList.Count} agents, {MusicList.Count} music kits");
    }

    internal static PluginConfig LoadConfigFromFile(string filePath, string sectionName)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, PluginConfig>>(json);
            if (dict != null && dict.TryGetValue(sectionName, out var config))
                return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Failed to load config: {ex.Message}");
        }
        return new PluginConfig();
    }

    internal static async Task CheckDatabaseTables()
    {
        if (Database is null) return;

        try
        {
            await using var connection = await Database.GetConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string[] createTableQueries =
                [
                    @"CREATE TABLE IF NOT EXISTS `wp_player_skins` (
                        `steamid` varchar(18) NOT NULL,
                        `weapon_team` int(1) NOT NULL,
                        `weapon_defindex` int(6) NOT NULL,
                        `weapon_paint_id` int(6) NOT NULL,
                        `weapon_wear` float NOT NULL DEFAULT 0.000001,
                        `weapon_seed` int(16) NOT NULL DEFAULT 0,
                        `weapon_nametag` VARCHAR(128) DEFAULT NULL,
                        `weapon_stattrak` tinyint(1) NOT NULL DEFAULT 0,
                        `weapon_stattrak_count` int(10) NOT NULL DEFAULT 0,
                        `weapon_sticker_0` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0',
                        `weapon_sticker_1` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0',
                        `weapon_sticker_2` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0',
                        `weapon_sticker_3` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0',
                        `weapon_sticker_4` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0;0;0',
                        `weapon_keychain` VARCHAR(128) NOT NULL DEFAULT '0;0;0;0;0',
                        UNIQUE (`steamid`, `weapon_team`, `weapon_defindex`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;",

                    @"CREATE TABLE IF NOT EXISTS `wp_player_knife` (
                        `steamid` varchar(18) NOT NULL,
                        `weapon_team` int(1) NOT NULL,
                        `knife` varchar(64) NOT NULL,
                        UNIQUE (`steamid`, `weapon_team`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;",

                    @"CREATE TABLE IF NOT EXISTS `wp_player_gloves` (
                        `steamid` varchar(18) NOT NULL,
                        `weapon_team` int(1) NOT NULL,
                        `weapon_defindex` int(11) NOT NULL,
                        UNIQUE (`steamid`, `weapon_team`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;",

                    @"CREATE TABLE IF NOT EXISTS `wp_player_agents` (
                        `steamid` varchar(18) NOT NULL,
                        `agent_ct` varchar(64) DEFAULT NULL,
                        `agent_t` varchar(64) DEFAULT NULL,
                        UNIQUE (`steamid`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;",

                    @"CREATE TABLE IF NOT EXISTS `wp_player_music` (
                        `steamid` varchar(64) NOT NULL,
                        `weapon_team` int(1) NOT NULL,
                        `music_id` int(11) NOT NULL,
                        UNIQUE (`steamid`, `weapon_team`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;"
                ];

                foreach (var query in createTableQueries)
                {
                    await connection.ExecuteAsync(query, transaction: transaction);
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("[OstoraWeaponSkins] Unable to create database tables!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Database error: {ex.Message}");
        }
    }

    internal static void LoadSkinsFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var deserializedSkins = JsonConvert.DeserializeObject<List<JObject>>(json);
            SkinsList = deserializedSkins ?? [];
        }
        catch (FileNotFoundException)
        {
            LogDebug($"[OstoraWeaponSkins] File not found: {filePath}");
        }
    }

    internal static void LoadGlovesFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            GlovesList = JsonConvert.DeserializeObject<List<JObject>>(json) ?? [];
        }
        catch (FileNotFoundException)
        {
            LogDebug($"[OstoraWeaponSkins] File not found: {filePath}");
        }
    }

    internal static void LoadAgentsFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var agents = JsonConvert.DeserializeObject<List<JObject>>(json) ?? [];
            AgentsList = agents;

            // Build index from image URL: https://.../agent-4732.png
            AgentIndexLookup.Clear();
            foreach (var agent in agents)
            {
                var img = agent["image"]?.ToString();
                if (string.IsNullOrEmpty(img)) continue;
                var match = Regex.Match(img, @"agent-(\d+)\.png");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var idx))
                    AgentIndexLookup[idx] = agent;
            }
            LogDebug($"[OstoraWeaponSkins] Agent index lookup built: {AgentIndexLookup.Count} entries");
        }
        catch (FileNotFoundException)
        {
            LogDebug($"[OstoraWeaponSkins] File not found: {filePath}");
        }
    }

    internal static void LoadMusicFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var deserialized = JsonConvert.DeserializeObject<List<JObject>>(json);
            MusicList = deserialized ?? [];
        }
        catch (FileNotFoundException)
        {
            LogDebug($"[OstoraWeaponSkins] File not found: {filePath}");
        }
    }

    internal static bool IsPlayerValid(IPlayer? player)
    {
        return player is { IsFakeClient: false, IsAuthorized: true };
    }

    internal static bool HasChangedKnife(IPlayer player, out string? knifeValue)
    {
        knifeValue = null;

        var controller = player.Controller;
        if (controller == null) return false;

        int team = controller.TeamNum;

        if (!GPlayersKnife.TryGetValue(player.Slot, out var knife) ||
            !knife.TryGetValue(team, out var value) ||
            value == "weapon_knife") return false;
        knifeValue = value;
        return true;
    }

    internal static bool HasChangedPaint(IPlayer player, int weaponDefIndex, out WeaponInfo? weaponInfo)
    {
        weaponInfo = null;

        var controller = player.Controller;
        if (controller == null) return false;

        int team = controller.TeamNum;

        if (!GPlayerWeaponsInfo.TryGetValue(player.Slot, out var teamInfo) ||
            !teamInfo.TryGetValue(team, out var teamWeapons))
        {
            return false;
        }

        if (!teamWeapons.TryGetValue(weaponDefIndex, out var value) || value.Paint <= 0) return false;

        weaponInfo = value;
        return true;
    }

    internal static float ViewAsFloat(uint value)
    {
        return BitConverter.Int32BitsToSingle((int)value);
    }

    #endregion
}
