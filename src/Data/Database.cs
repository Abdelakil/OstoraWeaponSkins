using FreeSql;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Database;
using SwiftlyS2.Shared.Players;

namespace OstoraWeaponSkins;

public class Database
{
    private IFreeSql Fsql { get; set; } = null!;
    private ISwiftlyCore Core { get; }

    public Database(ISwiftlyCore core) { Core = core; }

    public void Start(IDatabaseService dbService)
    {
        var info = dbService.GetConnectionInfo("OstoraWeaponskins");
        var protocol = info.Driver switch
        {
            "mysql" => DataType.MySql,
            "postgresql" => DataType.PostgreSQL,
            "sqlite" => DataType.Sqlite,
            _ => throw new Exception($"Unsupported DB driver: {info.Driver}"),
        };
        var conn = dbService.GetConnection("OstoraWeaponskins");
        Fsql = new FreeSqlBuilder()
            .UseConnectionString(protocol, conn.ConnectionString)
            .UseAdoConnectionPool(true)
            .Build();

        // Create tables if they don't exist
        CreateTablesIfNotExist();
    }

    private void CreateTablesIfNotExist()
    {
        try
        {
            // Check and create wp_player_skins
            if (!TableExists("wp_player_skins"))
            {
                Fsql.CodeFirst.SyncStructure<SkinModel>();
            }

            // Check and create wp_player_knife
            if (!TableExists("wp_player_knife"))
            {
                Fsql.CodeFirst.SyncStructure<KnifeModel>();
            }

            // Check and create wp_player_gloves
            if (!TableExists("wp_player_gloves"))
            {
                Fsql.CodeFirst.SyncStructure<GloveModel>();
            }

            // Check and create wp_player_agents
            if (!TableExists("wp_player_agents"))
            {
                Fsql.CodeFirst.SyncStructure<AgentModel>();
            }

            // Check and create wp_player_music
            if (!TableExists("wp_player_music"))
            {
                Fsql.CodeFirst.SyncStructure<MusicKitModel>();
            }
        }
        catch (Exception)
        {
            // Silently fail - tables might already exist with different schema
        }
    }

    private bool TableExists(string tableName)
    {
        try
        {
            var result = Fsql.Ado.ExecuteScalar($"SELECT 1 FROM information_schema.TABLES WHERE table_schema=DATABASE() and table_name='{tableName}'");
            return result != null && result != DBNull.Value;
        }
        catch
        {
            return false;
        }
    }

    // ── Weapon skins ────────────────────────────────────────────
    public async Task<List<WeaponSkinData>> GetWeaponSkinsAsync(ulong steamId)
    {
        var models = await Fsql.Select<SkinModel>()
            .Where(s => s.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Where(s => SkinUtils.IsWeapon(s.DefinitionIndex)).Select(s => s.ToWeaponData()).ToList();
    }

    // ── Knife skins ─────────────────────────────────────────────
    public async Task<List<KnifeSkinData>> GetKnifeSkinsAsync(ulong steamId)
    {
        var results = await Fsql.Select<KnifeModel, SkinModel>()
            .LeftJoin((k, s) => k.SteamID == s.SteamID && k.Team == s.Team)
            .Where((k, s) => k.SteamID == steamId.ToString())
            .ToListAsync((Knife, Skin) => new { Knife, Skin });

        return results
            .Where(r => r.Skin == null || r.Knife.Knife == Core.Helpers.GetClassnameByDefinitionIndex(r.Skin.DefinitionIndex))
            .Select(r =>
            {
                var defIndex = (ushort)Core.Helpers.GetDefinitionIndexByClassname(r.Knife.Knife)!.Value;
                if (r.Skin != null)
                    return r.Skin.ToKnifeData(defIndex);
                return new KnifeSkinData { SteamID = ulong.Parse(r.Knife.SteamID), Team = (Team)r.Knife.Team, DefinitionIndex = defIndex };
            }).ToList();
    }

    // ── Glove skins ─────────────────────────────────────────────
    public async Task<List<GloveData>> GetGloveSkinsAsync(ulong steamId)
    {
        var results = await Fsql.Select<GloveModel, SkinModel>()
            .LeftJoin((g, s) => g.SteamID == s.SteamID && g.Team == s.Team && g.DefinitionIndex == s.DefinitionIndex)
            .Where((g, s) => g.SteamID == steamId.ToString())
            .ToListAsync((Glove, Skin) => new { Glove, Skin });

        return results.Select(r =>
        {
            var data = new GloveData
            {
                SteamID = ulong.Parse(r.Glove.SteamID),
                Team = (Team)r.Glove.Team,
                DefinitionIndex = (ushort)r.Glove.DefinitionIndex,
            };
            if (r.Skin != null)
            {
                data.Paintkit = r.Skin.PaintID;
                data.PaintkitWear = r.Skin.Wear;
                data.PaintkitSeed = r.Skin.Seed;
            }
            return data;
        }).ToList();
    }

    // ── Agents ──────────────────────────────────────────────────
    public async Task<List<(Team Team, int AgentIndex)>> GetAgentsAsync(ulong steamId)
    {
        var models = await Fsql.Select<AgentModel>()
            .Where(a => a.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => ((Team)m.Team, m.AgentIndex)).ToList();
    }

    // ── Music kits ──────────────────────────────────────────────
    public async Task<List<MusicKitData>> GetMusicKitAsync(ulong steamId)
    {
        var models = await Fsql.Select<MusicKitModel>()
            .Where(m => m.SteamID == steamId.ToString())
            .ToListAsync();
        return models.Select(m => new MusicKitData
        {
            SteamID = steamId,
            Team = (Team)m.WeaponTeam,
            MusicID = m.MusicID
        }).ToList();
    }
}
