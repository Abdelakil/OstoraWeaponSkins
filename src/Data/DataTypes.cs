using SwiftlyS2.Shared.Players;

namespace OstoraWeaponSkins;

// ── Weapon skin data ────────────────────────────────────────────
public record WeaponSkinData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; init; }
    public required ushort DefinitionIndex { get; init; }
    public EconItemQuality Quality { get; set; } = EconItemQuality.Normal;
    public int Paintkit { get; set; }
    public int PaintkitSeed { get; set; }
    public float PaintkitWear { get; set; }
    public string? Nametag { get; set; }
    public int StattrakCount { get; set; }
    public StickerData? Sticker0 { get; set; }
    public StickerData? Sticker1 { get; set; }
    public StickerData? Sticker2 { get; set; }
    public StickerData? Sticker3 { get; set; }
    public StickerData? Sticker4 { get; set; }
    public KeychainData? Keychain0 { get; set; }

    public StickerData? GetSticker(int slot) => slot switch
    {
        0 => Sticker0, 1 => Sticker1, 2 => Sticker2,
        3 => Sticker3, 4 => Sticker4,
        _ => null
    };

    public void SetSticker(int slot, StickerData? data)
    {
        switch (slot)
        {
            case 0: Sticker0 = data; break;
            case 1: Sticker1 = data; break;
            case 2: Sticker2 = data; break;
            case 3: Sticker3 = data; break;
            case 4: Sticker4 = data; break;
        }
    }

    public bool HasSticker(int slot) => GetSticker(slot) != null;
}

// ── Knife skin data ─────────────────────────────────────────────
public record KnifeSkinData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; set; }
    public required ushort DefinitionIndex { get; set; }
    public EconItemQuality Quality { get; set; } = EconItemQuality.Normal;
    public string? Nametag { get; set; }
    public int StattrakCount { get; set; }
    public int Paintkit { get; set; }
    public int PaintkitSeed { get; set; }
    public float PaintkitWear { get; set; }
}

// ── Glove data ──────────────────────────────────────────────────
public record GloveData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; set; }
    public required ushort DefinitionIndex { get; set; }
    public int Paintkit { get; set; }
    public int PaintkitSeed { get; set; }
    public float PaintkitWear { get; set; }
}

// ── Music kit data ───────────────────────────────────────────────
public record MusicKitData
{
    public required ulong SteamID { get; set; }
    public required Team Team { get; set; }
    public required int MusicID { get; set; }
}

// ── Sticker data ────────────────────────────────────────────────
public record StickerData
{
    public required int Id { get; set; }
    public float Wear { get; set; }
    public float Scale { get; set; } = 1f;
    public float Rotation { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public int Schema { get; set; } = 1337;

    public StickerData DeepClone()
    {
        return new StickerData
        {
            Id = Id,
            Wear = Wear,
            Scale = Scale,
            Rotation = Rotation,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Schema = Schema,
        };
    }
}

// ── Keychain data ───────────────────────────────────────────────
public record KeychainData
{
    public required int Id { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public float OffsetZ { get; set; }
    public int Seed { get; set; }
}

// ── Enums ───────────────────────────────────────────────────────
public enum EconItemQuality : byte
{
    Normal = 0,
    Genuine = 1,
    Vintage = 2,
    Unusual = 3,
    Community = 5,
    Developer = 6,
    SelfMade = 7,
    Customized = 8,
    Strange = 9,
    Completed = 10,
    Haunted = 11,
    Tournament = 12,
    Highlight = 13,
    Volatile = 14,
    StatTrak = 9,
    Souvenir = 12
}

public enum AttributeDefinitionIndex : ushort
{
    SET_ITEM_TEXTURE_PREFAB = 6,
    SET_ITEM_TEXTURE_SEED = 7,
    SET_ITEM_TEXTURE_WEAR = 8,
    KILL_EATER = 80,
    KILL_EATER_SCORE_TYPE = 81,
    CUSTOM_NAME_ATTR = 111,
    STICKER_SLOT_0_ID = 113, STICKER_SLOT_0_WEAR = 114, STICKER_SLOT_0_SCALE = 115, STICKER_SLOT_0_ROTATION = 116,
    STICKER_SLOT_1_ID = 117, STICKER_SLOT_1_WEAR = 118, STICKER_SLOT_1_SCALE = 119, STICKER_SLOT_1_ROTATION = 120,
    STICKER_SLOT_2_ID = 121, STICKER_SLOT_2_WEAR = 122, STICKER_SLOT_2_SCALE = 123, STICKER_SLOT_2_ROTATION = 124,
    STICKER_SLOT_3_ID = 125, STICKER_SLOT_3_WEAR = 126, STICKER_SLOT_3_SCALE = 127, STICKER_SLOT_3_ROTATION = 128,
    STICKER_SLOT_4_ID = 129, STICKER_SLOT_4_WEAR = 130, STICKER_SLOT_4_SCALE = 131, STICKER_SLOT_4_ROTATION = 132,
    STICKER_SLOT_5_ID = 133, STICKER_SLOT_5_WEAR = 134, STICKER_SLOT_5_SCALE = 135, STICKER_SLOT_5_ROTATION = 136,
    MUSIC_ID = 166,
    STICKER_SLOT_0_OFFSET_X = 278, STICKER_SLOT_0_OFFSET_Y = 279,
    STICKER_SLOT_1_OFFSET_X = 280, STICKER_SLOT_1_OFFSET_Y = 281,
    STICKER_SLOT_2_OFFSET_X = 282, STICKER_SLOT_2_OFFSET_Y = 283,
    STICKER_SLOT_3_OFFSET_X = 284, STICKER_SLOT_3_OFFSET_Y = 285,
    STICKER_SLOT_4_OFFSET_X = 286, STICKER_SLOT_4_OFFSET_Y = 287,
    STICKER_SLOT_5_OFFSET_X = 288, STICKER_SLOT_5_OFFSET_Y = 289,
    STICKER_SLOT_0_SCHEMA = 290, STICKER_SLOT_1_SCHEMA = 291,
    STICKER_SLOT_2_SCHEMA = 292, STICKER_SLOT_3_SCHEMA = 293,
    STICKER_SLOT_4_SCHEMA = 294, STICKER_SLOT_5_SCHEMA = 295,
    KEYCHAIN_SLOT_0_ID = 299, KEYCHAIN_SLOT_0_OFFSET_X = 300,
    KEYCHAIN_SLOT_0_OFFSET_Y = 301, KEYCHAIN_SLOT_0_OFFSET_Z = 302,
    KEYCHAIN_SLOT_0_SEED = 306,
}

// ── Utility ─────────────────────────────────────────────────────
public static class SkinUtils
{
    public static bool IsKnife(int def) => def is 42 or 59 or (>= 500 and < 600);
    public static bool IsGlove(int def) => def > 5000;
    public static bool IsWeapon(int def) => !IsKnife(def) && !IsGlove(def) && def < 100;
}
