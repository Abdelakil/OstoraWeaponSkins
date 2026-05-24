using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using MySqlConnector;
using SwiftlyS2.Shared.Players;

namespace OstoraWeaponSkins;

internal sealed class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        // Strip keys MySqlConnector doesn't recognize (e.g. "timeout" from SwiftlyS2's database.jsonc)
        var clean = System.Text.RegularExpressions.Regex.Replace(
            connectionString, @"timeout=[^;]*;?", "", RegexOptions.IgnoreCase);
        var builder = new MySqlConnectionStringBuilder(clean);
        _connectionString = builder.ConnectionString;
    }

    public async Task<MySqlConnection> GetConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task LoadPlayerData(IPlayer player)
    {
        if (player.IsFakeClient) return;

        try
        {
            var steamId = player.SteamID.ToString();
            Console.WriteLine($"[OstoraWeaponSkins] DB: Loading data for SteamID={steamId}, connection={OstoraWeaponSkins.GetConfig().DatabaseConnection}");

            await using var connection = await GetConnectionAsync();
            Console.WriteLine($"[OstoraWeaponSkins] DB: Connection opened. Server={connection.DataSource}, DB={connection.Database}");

            var config = OstoraWeaponSkins.GetConfig();

            if (config.KnifeEnabled)
                await GetKnifeFromDatabase(player, connection);
            if (config.GloveEnabled)
                await GetGloveFromDatabase(player, connection);
            if (config.AgentEnabled)
                await GetAgentFromDatabase(player, connection);
            if (config.MusicEnabled)
                await GetMusicFromDatabase(player, connection);
            if (config.SkinEnabled)
                await GetWeaponPaintsFromDatabase(player, connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error loading player data: {ex.Message}");
        }
    }

    private async Task GetKnifeFromDatabase(IPlayer player, MySqlConnection connection)
    {
        try
        {
            if (!OstoraWeaponSkins.GetConfig().KnifeEnabled) return;

            const string query = "SELECT `knife`, `weapon_team` FROM `wp_player_knife` WHERE `steamid` = @steamid ORDER BY `weapon_team` ASC";
            var rows = connection.Query<dynamic>(query, new { steamid = player.SteamID.ToString() });
            Console.WriteLine($"[OstoraWeaponSkins] DB: GetKnife returned {rows.Count()} rows for steamid={player.SteamID}");

            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.knife)) continue;

                int weaponTeam = (int)row.weapon_team switch
                {
                    2 => 2,
                    3 => 3,
                    _ => 0,
                };

                var playerKnives = OstoraWeaponSkins.GPlayersKnife.GetOrAdd(player.Slot, _ => new ConcurrentDictionary<int, string>());

                if (weaponTeam == 0)
                {
                    playerKnives[2] = row.knife;
                    playerKnives[3] = row.knife;
                }
                else
                {
                    playerKnives[weaponTeam] = row.knife;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in GetKnifeFromDatabase: {ex.Message}");
        }
    }

    private async Task GetGloveFromDatabase(IPlayer player, MySqlConnection connection)
    {
        try
        {
            if (!OstoraWeaponSkins.GetConfig().GloveEnabled) return;

            const string query = "SELECT `weapon_defindex`, `weapon_team` FROM `wp_player_gloves` WHERE `steamid` = @steamid ORDER BY `weapon_team` ASC";
            var rows = connection.Query<dynamic>(query, new { steamid = player.SteamID.ToString() });
            Console.WriteLine($"[OstoraWeaponSkins] DB: GetGlove returned {rows.Count()} rows");

            foreach (var row in rows)
            {
                if (row.weapon_defindex == null) continue;

                var playerGloves = OstoraWeaponSkins.GPlayersGlove.GetOrAdd(player.Slot, _ => new ConcurrentDictionary<int, ushort>());
                int weaponTeam = (int)row.weapon_team switch
                {
                    2 => 2,
                    3 => 3,
                    _ => 0,
                };

                if (weaponTeam == 0)
                {
                    playerGloves[2] = (ushort)row.weapon_defindex;
                    playerGloves[3] = (ushort)row.weapon_defindex;
                }
                else
                {
                    playerGloves[weaponTeam] = (ushort)row.weapon_defindex;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in GetGloveFromDatabase: {ex.Message}");
        }
    }

    private async Task GetAgentFromDatabase(IPlayer player, MySqlConnection connection)
    {
        try
        {
            if (!OstoraWeaponSkins.GetConfig().AgentEnabled) return;

            const string query = "SELECT `weapon_team`, `agent_index` FROM `wp_player_agents` WHERE `steamid` = @steamid";
            var rows = connection.Query<dynamic>(query, new { steamid = player.SteamID.ToString() });
            Console.WriteLine($"[OstoraWeaponSkins] DB: GetAgent returned {rows.Count()} rows");

            string? agentCT = null;
            string? agentT = null;

            foreach (var row in rows)
            {
                int team = (int)row.weapon_team;
                var agentIndex = row.agent_index?.ToString();
                if (string.IsNullOrEmpty(agentIndex)) continue;

                var agent = OstoraWeaponSkins.AgentsList?.FirstOrDefault(a =>
                {
                    var img = a["image"]?.ToString() ?? "";
                    var imgTeam = a["team"]?.ToObject<int>() ?? 0;
                    return img.Contains($"agent-{agentIndex}.png") && imgTeam == team;
                });
                var model = agent?["model"]?.ToString();
                if (model == "null") model = null;

                if (team == 2) agentT = model;
                else if (team == 3) agentCT = model;
            }

            if (!string.IsNullOrEmpty(agentCT) || !string.IsNullOrEmpty(agentT))
            {
                OstoraWeaponSkins.GPlayersAgent[player.Slot] = (agentCT, agentT);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in GetAgentFromDatabase: {ex.Message}");
        }
    }

    private async Task GetWeaponPaintsFromDatabase(IPlayer player, MySqlConnection connection)
    {
        try
        {
            if (!OstoraWeaponSkins.GetConfig().SkinEnabled || player == null) return;

            var playerWeapons = OstoraWeaponSkins.GPlayerWeaponsInfo.GetOrAdd(player.Slot,
                _ => new ConcurrentDictionary<int, ConcurrentDictionary<int, WeaponInfo>>());

            const string query = "SELECT * FROM `wp_player_skins` WHERE `steamid` = @steamid ORDER BY `weapon_team` ASC";
            var playerSkins = connection.Query<dynamic>(query, new { steamid = player.SteamID.ToString() });

            foreach (var row in playerSkins)
            {
                int weaponDefIndex = row.weapon_defindex ?? 0;
                int weaponPaintId = row.weapon_paint_id ?? 0;
                float weaponWear = row.weapon_wear ?? 0f;
                int weaponSeed = row.weapon_seed ?? 0;
                string weaponNameTag = row.weapon_nametag ?? "";
                bool weaponStatTrak = Convert.ToBoolean(row.weapon_stattrak);
                int weaponStatTrakCount = row.weapon_stattrak_count ?? 0;

                int weaponTeam = (int)row.weapon_team;
                if (weaponTeam != 2 && weaponTeam != 3) weaponTeam = 0;

                string[]? keyChainParts = row.weapon_keychain?.ToString().Split(';');

                var keyChainInfo = new KeyChainInfo();

                if (keyChainParts!.Length == 5 &&
                    uint.TryParse(keyChainParts[0], out uint keyChainId) &&
                    float.TryParse(keyChainParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float keyChainOffsetX) &&
                    float.TryParse(keyChainParts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float keyChainOffsetY) &&
                    float.TryParse(keyChainParts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float keyChainOffsetZ) &&
                    uint.TryParse(keyChainParts[4], out uint keyChainSeed))
                {
                    keyChainInfo.Id = keyChainId;
                    keyChainInfo.OffsetX = keyChainOffsetX;
                    keyChainInfo.OffsetY = keyChainOffsetY;
                    keyChainInfo.OffsetZ = keyChainOffsetZ;
                    keyChainInfo.Seed = keyChainSeed;
                }

                var weaponInfo = new WeaponInfo
                {
                    Paint = weaponPaintId,
                    Seed = weaponSeed,
                    Wear = weaponWear,
                    Nametag = weaponNameTag,
                    KeyChain = keyChainInfo,
                    StatTrak = weaponStatTrak,
                    StatTrakCount = weaponStatTrakCount,
                };

                for (int i = 0; i <= 4; i++)
                {
                    string stickerColumn = $"weapon_sticker_{i}";
                    var stickerData = ((IDictionary<string, object>)row!)[stickerColumn];

                    if (string.IsNullOrEmpty(stickerData.ToString())) continue;

                    var parts = stickerData.ToString()!.Split(';');

                    if (parts.Length != 7 ||
                        !uint.TryParse(parts[0], out uint stickerId) ||
                        !uint.TryParse(parts[1], out uint stickerSchema) ||
                        !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float stickerOffsetX) ||
                        !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float stickerOffsetY) ||
                        !float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float stickerWear) ||
                        !float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float stickerScale) ||
                        !float.TryParse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float stickerRotation)) continue;

                    weaponInfo.Stickers.Add(new StickerInfo
                    {
                        Id = stickerId,
                        Schema = stickerSchema,
                        OffsetX = stickerOffsetX,
                        OffsetY = stickerOffsetY,
                        Wear = stickerWear,
                        Scale = stickerScale,
                        Rotation = stickerRotation
                    });
                }

                if (weaponTeam == 0)
                {
                    var terroristWeapons = playerWeapons.GetOrAdd(2, _ => new ConcurrentDictionary<int, WeaponInfo>());
                    var counterTerroristWeapons = playerWeapons.GetOrAdd(3, _ => new ConcurrentDictionary<int, WeaponInfo>());
                    terroristWeapons[weaponDefIndex] = weaponInfo;
                    counterTerroristWeapons[weaponDefIndex] = weaponInfo;
                }
                else
                {
                    var teamWeapons = playerWeapons.GetOrAdd(weaponTeam, _ => new ConcurrentDictionary<int, WeaponInfo>());
                    teamWeapons[weaponDefIndex] = weaponInfo;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in GetWeaponPaintsFromDatabase: {ex.Message}");
        }
    }

    private async Task GetMusicFromDatabase(IPlayer player, MySqlConnection connection)
    {
        try
        {
            if (!OstoraWeaponSkins.GetConfig().MusicEnabled) return;

            const string query = "SELECT `music_id`, `weapon_team` FROM `wp_player_music` WHERE `steamid` = @steamid ORDER BY `weapon_team` ASC";
            var rows = connection.Query<dynamic>(query, new { steamid = player.SteamID.ToString() });

            foreach (var row in rows)
            {
                if (row.music_id == null) continue;

                int weaponTeam = (int)row.weapon_team switch
                {
                    2 => 2,
                    3 => 3,
                    _ => 0,
                };

                var playerMusic = OstoraWeaponSkins.GPlayersMusic.GetOrAdd(player.Slot, _ => new ConcurrentDictionary<int, ushort>());

                if (weaponTeam == 0)
                {
                    playerMusic[2] = (ushort)row.music_id;
                    playerMusic[3] = (ushort)row.music_id;
                }
                else
                {
                    playerMusic[weaponTeam] = (ushort)row.music_id;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in GetMusicFromDatabase: {ex.Message}");
        }
    }
}
