using FreeSql.DataAnnotations;
using SwiftlyS2.Shared.Players;

namespace OstoraWeaponSkins;

// ── wp_player_skins ─────────────────────────────────────────────
[Table(Name = "wp_player_skins")]
[Index("steamid", "steamid,weapon_team,weapon_defindex")]
public record SkinModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "weapon_defindex")] public required int DefinitionIndex { get; set; }
    [Column(Name = "weapon_paint_id")] public required int PaintID { get; set; }
    [Column(Name = "weapon_wear")] public float Wear { get; set; } = 0.000001f;
    [Column(Name = "weapon_seed")] public int Seed { get; set; }
    [Column(Name = "weapon_nametag")] public string? Nametag { get; set; }
    [Column(Name = "weapon_stattrak")] public bool Stattrak { get; set; }
    [Column(Name = "weapon_stattrak_count")] public int StattrakCount { get; set; }
    [Column(Name = "weapon_sticker_0")] public string Sticker0 { get; set; } = "0;0;0;0;0;0;0";
    [Column(Name = "weapon_sticker_1")] public string Sticker1 { get; set; } = "0;0;0;0;0;0;0";
    [Column(Name = "weapon_sticker_2")] public string Sticker2 { get; set; } = "0;0;0;0;0;0;0";
    [Column(Name = "weapon_sticker_3")] public string Sticker3 { get; set; } = "0;0;0;0;0;0;0";
    [Column(Name = "weapon_sticker_4")] public string Sticker4 { get; set; } = "0;0;0;0;0;0;0";
    [Column(Name = "weapon_keychain")] public string Keychain { get; set; } = "0;0;0;0;0";

    private static StickerData? ParseSticker(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        
        var p = s.Split(';');
        if (p.Length < 7 || p[0] == "0") return null;
        return new StickerData
        {
            Id = int.Parse(p[0]), Schema = int.Parse(p[1]),
            OffsetX = float.Parse(p[2]), OffsetY = float.Parse(p[3]),
            Wear = float.Parse(p[4]), Scale = float.Parse(p[5]),
            Rotation = float.Parse(p[6]),
        };
    }

    private static KeychainData? ParseKeychain(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        
        var p = s.Split(';');
        if (p.Length < 5 || p[0] == "0") return null;
        return new KeychainData
        {
            Id = int.Parse(p[0]),
            OffsetX = float.Parse(p[1]), OffsetY = float.Parse(p[2]),
            OffsetZ = float.Parse(p[3]), Seed = int.Parse(p[4]),
        };
    }

    public WeaponSkinData ToWeaponData() => new()
    {
        SteamID = ulong.Parse(SteamID), Team = (Team)Team,
        DefinitionIndex = (ushort)DefinitionIndex,
        Paintkit = PaintID, PaintkitWear = Wear, PaintkitSeed = Seed,
        Nametag = Nametag,
        Quality = Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Normal,
        StattrakCount = StattrakCount,
        Sticker0 = ParseSticker(Sticker0), Sticker1 = ParseSticker(Sticker1),
        Sticker2 = ParseSticker(Sticker2), Sticker3 = ParseSticker(Sticker3),
        Sticker4 = ParseSticker(Sticker4),
        Keychain0 = ParseKeychain(Keychain),
    };

    public KnifeSkinData ToKnifeData(ushort knifeDefIndex) => new()
    {
        SteamID = ulong.Parse(SteamID), Team = (Team)Team,
        DefinitionIndex = knifeDefIndex,
        Paintkit = PaintID, PaintkitWear = Wear, PaintkitSeed = Seed,
        Nametag = Nametag,
        Quality = Stattrak ? EconItemQuality.StatTrak : EconItemQuality.Unusual,
        StattrakCount = StattrakCount,
    };

    public GloveData ToGloveData() => new()
    {
        SteamID = ulong.Parse(SteamID), Team = (Team)Team,
        DefinitionIndex = (ushort)DefinitionIndex,
        Paintkit = PaintID, PaintkitWear = Wear, PaintkitSeed = Seed,
    };
}

// ── wp_player_knife ─────────────────────────────────────────────
[Table(Name = "wp_player_knife")]
[Index("steamid", "steamid,weapon_team")]
public record KnifeModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "knife")] public required string Knife { get; set; }
}

// ── wp_player_gloves ────────────────────────────────────────────
[Table(Name = "wp_player_gloves")]
[Index("steamid", "steamid,weapon_team")]
public record GloveModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "weapon_defindex")] public required int DefinitionIndex { get; set; }
}

// ── wp_player_agents ────────────────────────────────────────────
[Table(Name = "wp_player_agents")]
[Index("steamid", "steamid,weapon_team")]
public record AgentModel
{
    [Column(Name = "steamid")] public required string SteamID { get; set; }
    [Column(Name = "weapon_team")] public required short Team { get; set; }
    [Column(Name = "agent_index")] public required int AgentIndex { get; set; }
}

// ── wp_player_music ─────────────────────────────────────────────
[Table(Name = "wp_player_music")]
public class MusicKitModel
{
    [Column(Name = "steamid", IsPrimary = true)] public string SteamID { get; set; } = string.Empty;
    [Column(Name = "weapon_team", IsPrimary = true)] public int WeaponTeam { get; set; }
    [Column(Name = "music_id")] public int MusicID { get; set; }
}
