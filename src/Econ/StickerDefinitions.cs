namespace OstoraWeaponSkins.Econ;

// ── Sticker definition ─────────────────────────────────────────
public record StickerDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required string ItemName { get; init; }
    public required string StickerMaterial { get; init; }
}

// ── Sticker collection definition ─────────────────────────────
public record StickerCollectionDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required string ItemName { get; init; }
    public required List<StickerDefinition> Stickers { get; init; }
}
