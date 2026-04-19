using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace OstoraWeaponSkins;

// ── SOID_t ──────────────────────────────────────────────────────
public struct SOID_t
{
    private ulong m_id;
    private uint m_type;
    private uint m_padding;

    public SOID_t(ulong steamid) { m_id = steamid; m_type = 1; m_padding = 0; }
    public SOID_t(ulong soid1, ulong soid2) { m_id = soid1; m_type = (uint)soid2; m_padding = 0; }
    public ulong SteamID => m_id;
    public ulong Part1 => m_id;
    public ulong Part2 => m_type;
}

// ── Attribute ───────────────────────────────────────────────────
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Attribute
{
    [FieldOffset(0)] public AttributeDefinitionIndex AttributeDefinitionIndex;
    [FieldOffset(8)] public float FloatData;
    [FieldOffset(8)] public int IntData;
    [FieldOffset(8)] public nint PtrData;
}

// ── CustomAttributeData ─────────────────────────────────────────
public class CustomAttributeData : INativeHandle
{
    [SwiftlyInject] private static ISwiftlyCore Core { get; set; } = null!;

    public nint Address { get; set; }
    public bool IsValid => Address != 0;

    public CustomAttributeData(nint address) { Address = address; }

    private ref byte _flags => ref Address.AsRef<byte>(0);
    public ref byte Count => ref Address.AsRef<byte>(2);

    public static CustomAttributeData Create()
    {
        var addr = Core.Memory.Alloc(4);
        var data = new CustomAttributeData(addr);
        data._flags = 0x3F;
        data.Count = 0;
        return data;
    }

    public void AddAttribute(Attribute attribute)
    {
        Address = Core.Memory.Resize(Address, (ulong)(4 + (Count + 1) * Unsafe.SizeOf<Attribute>()));
        this[Count] = attribute;
        Count++;
    }

    public void UpdateAttribute(Attribute attribute)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].AttributeDefinitionIndex == attribute.AttributeDefinitionIndex)
            {
                this[i] = attribute;
                return;
            }
        }
        AddAttribute(attribute);
    }

    public ref Attribute this[int index] => ref Address.AsRef<Attribute>(4 + index * Unsafe.SizeOf<Attribute>());

    public void SetPaintkit(int paintkit) =>
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_PREFAB, FloatData = Convert.ToSingle(paintkit) });
    public void SetPaintkitSeed(int seed) =>
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_SEED, IntData = seed });
    public void SetPaintkitWear(float wear) =>
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.SET_ITEM_TEXTURE_WEAR, FloatData = wear });
    public void SetStattrak(int count)
    {
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KILL_EATER, IntData = count });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KILL_EATER_SCORE_TYPE, IntData = 0 });
    }
    public void SetCustomName(string name)
    {
        var str = Natives.Instance.CreateAttributeString();
        str.Value = name;
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.CUSTOM_NAME_ATTR, PtrData = str.Address });
    }
    public void SetMusicId(int musicId) =>
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.MUSIC_ID, IntData = musicId });

    public void SetSticker(int slot, StickerData s)
    {
        int baseId = (int)AttributeDefinitionIndex.STICKER_SLOT_0_ID + slot * 4;
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)baseId, IntData = s.Id });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)(baseId + 1), FloatData = s.Wear });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)(baseId + 2), FloatData = s.Scale });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)(baseId + 3), FloatData = s.Rotation });
        if (s.Schema != 1337)
        {
            int offsetXBase = (int)AttributeDefinitionIndex.STICKER_SLOT_0_OFFSET_X + slot * 2;
            int schemaBase = (int)AttributeDefinitionIndex.STICKER_SLOT_0_SCHEMA + slot;
            UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)offsetXBase, FloatData = s.OffsetX });
            UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)(offsetXBase + 1), FloatData = s.OffsetY });
            UpdateAttribute(new Attribute { AttributeDefinitionIndex = (AttributeDefinitionIndex)schemaBase, IntData = s.Schema });
        }
    }

    public void SetKeychain(KeychainData k)
    {
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_ID, IntData = k.Id });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_X, FloatData = k.OffsetX });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_Y, FloatData = k.OffsetY });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_OFFSET_Z, FloatData = k.OffsetZ });
        UpdateAttribute(new Attribute { AttributeDefinitionIndex = AttributeDefinitionIndex.KEYCHAIN_SLOT_0_SEED, IntData = k.Seed });
    }
}

// ── CGCClientSharedObjectCache ──────────────────────────────────
public struct CGCClientSharedObjectCache(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0;
    public void AddObject(CEconItem item) => Natives.Instance.SOCache_AddObject.Call(Address, item.Address);
    public void RemoveObject(CEconItem item) => Natives.Instance.SOCache_RemoveObject.Call(Address, item.Address);
    public SOID_t Owner => Address.AsRef<SOID_t>(Natives.Instance.CGCClientSharedObjectCache_m_OwnerOffset);
}

// ── CEconItem ───────────────────────────────────────────────────
[StructLayout(LayoutKind.Explicit, Size = 72)]
public ref struct CEconItemStruct
{
    [FieldOffset(0x10)] public ulong ItemID;
    [FieldOffset(0x20)] public nint pCustomData;
    [FieldOffset(0x28)] public uint AccountID;
    [FieldOffset(0x2C)] public uint InventoryPosition;
    [FieldOffset(0x30)] public ushort DefinitionIndex;
    [FieldOffset(0x32)] private ushort _packedBits;

    public ushort Quality
    {
        get => (ushort)((_packedBits >> 5) & 0xF);
        set => _packedBits = (ushort)((_packedBits & ~(0xF << 5)) | ((value & 0xF) << 5));
    }
}

public class CEconItem : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;
    public CEconItem(nint address) { Address = address; }

    private ref CEconItemStruct S => ref Address.AsRef<CEconItemStruct>();

    public ulong ItemID { get => S.ItemID; set => S.ItemID = value; }
    public nint pCustomData { get => S.pCustomData; set => S.pCustomData = value; }
    public uint AccountID { get => S.AccountID; set => S.AccountID = value; }
    public uint InventoryPosition { get => S.InventoryPosition; set => S.InventoryPosition = value; }
    public ushort DefinitionIndex { get => S.DefinitionIndex; set => S.DefinitionIndex = value; }
    public EconItemQuality Quality { get => (EconItemQuality)S.Quality; set => S.Quality = (byte)value; }

    public void ConfigureAttributes(Action<CustomAttributeData> configure)
    {
        if (pCustomData == 0)
            pCustomData = CustomAttributeData.Create().Address;
        var customData = new CustomAttributeData(pCustomData);
        configure(customData);
        pCustomData = customData.Address;
    }

    public void Apply(WeaponSkinData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        Quality = data.Quality;
        ConfigureAttributes(cd =>
        {
            cd.SetPaintkit(data.Paintkit);
            cd.SetPaintkitSeed(data.PaintkitSeed);
            cd.SetPaintkitWear(data.PaintkitWear);
            if (data.Quality == EconItemQuality.StatTrak) cd.SetStattrak(data.StattrakCount);
            if (data.Nametag != null) cd.SetCustomName(data.Nametag);
            for (int i = 0; i < 5; i++) { var s = data.GetSticker(i); if (s != null) cd.SetSticker(i, s); }
            if (data.Keychain0 != null) cd.SetKeychain(data.Keychain0);
        });
    }

    public void Apply(KnifeSkinData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        Quality = data.Quality;
        ConfigureAttributes(cd =>
        {
            cd.SetPaintkit(data.Paintkit);
            cd.SetPaintkitSeed(data.PaintkitSeed);
            cd.SetPaintkitWear(data.PaintkitWear);
            if (data.Quality == EconItemQuality.StatTrak) cd.SetStattrak(data.StattrakCount);
            if (data.Nametag != null) cd.SetCustomName(data.Nametag);
        });
    }

    public void Apply(GloveData data)
    {
        DefinitionIndex = data.DefinitionIndex;
        ConfigureAttributes(cd =>
        {
            cd.SetPaintkit(data.Paintkit);
            cd.SetPaintkitSeed(data.PaintkitSeed);
            cd.SetPaintkitWear(data.PaintkitWear);
        });
    }
}

// ── Loadout structs ─────────────────────────────────────────────
public struct LoadoutItem
{
    public ulong ItemId;
    public ushort DefinitionIndex;
}

[InlineArray(57)]
public struct LoadoutSlots { private LoadoutItem _element0; }

[InlineArray(4)]
public struct LoadoutTeams { private LoadoutSlots _element0; }

public struct CCSPlayerInventory_Loadouts
{
    private LoadoutTeams _element0;
    [UnscopedRef] public ref LoadoutItem this[Team team, loadout_slot_t slot] => ref this[(int)team, (int)slot];
    [UnscopedRef] public ref LoadoutItem this[Team team, int slot] => ref this[(int)team, slot];
    [UnscopedRef] public ref LoadoutItem this[int team, loadout_slot_t slot] => ref this[team, (int)slot];
    [UnscopedRef] public ref LoadoutItem this[int team, int slot] => ref _element0[team][slot];
}

// ── CCSInventoryManager ─────────────────────────────────────────
public struct CCSInventoryManager : INativeHandle
{
    public nint Address { get; set; }
    public bool IsValid => Address != 0;
    public CCSInventoryManager(nint address) { Address = address; }

    private nint DefaultLoadoutsStart => Address + Natives.Instance.CCSInventoryManager_m_DefaultLoadoutsOffset;

    public IEnumerable<(loadout_slot_t, CEconItemView)> GetDefaultLoadouts(Team team)
    {
        var start = DefaultLoadoutsStart;
        return Enumerable.Range(0, (int)loadout_slot_t.LOADOUT_SLOT_COUNT).Select(slot =>
            ((loadout_slot_t)slot,
             Helper.AsSchema<CEconItemView>(start +
                ((int)team * (int)loadout_slot_t.LOADOUT_SLOT_COUNT + slot) *
                Helper.GetSchemaSize<CEconItemView>())));
    }
}

// ── CCSPlayerInventory ──────────────────────────────────────────
public class CCSPlayerInventory : INativeHandle
{
    [SwiftlyInject] private static ISwiftlyCore Core { get; set; } = null!;

    public nint Address { get; set; }
    public bool IsValid => Address != 0 && SOCache.IsValid;
    public ulong SteamID => SOCache.Owner.SteamID;

    private CCSPlayerInventory_Loadouts _defaultLoadouts;

    public CCSPlayerInventory(nint address)
    {
        Address = address;
        _defaultLoadouts = Loadouts;
    }

    public CGCClientSharedObjectCache SOCache =>
        new(Address.Read<nint>(Natives.Instance.CCSPlayerInventory_m_pSOCacheOffset));

    public ref CUtlVector<PointerTo<CEconItemView>> Items =>
        ref Address.AsRef<CUtlVector<PointerTo<CEconItemView>>>(Natives.Instance.CCSPlayerInventory_m_ItemsOffset);

    public ref CCSPlayerInventory_Loadouts Loadouts =>
        ref Address.AsRef<CCSPlayerInventory_Loadouts>(Natives.Instance.CCSPlayerInventory_LoadoutsOffset);

    public CEconItemView? GetItemInLoadout(Team team, loadout_slot_t slot)
    {
        var ptr = Natives.Instance.CPlayerInventory_GetItemInLoadout.CallOriginal(Address, (int)team, (int)slot);
        return ptr != 0 ? Helper.AsSchema<CEconItemView>(ptr) : null;
    }

    private ulong GetHighestItemID() =>
        Items.Select(i => i.Value.ItemID).Where(IsValidItemID).DefaultIfEmpty(0UL).Max();

    private uint GetHighestInventoryPosition() =>
        Items.Select(i => i.Value.InventoryPosition).DefaultIfEmpty(0U).Max();

    private ulong GetNewItemID() => GetHighestItemID() + 1;
    private uint GetNewInventoryPosition() => GetHighestInventoryPosition() + 1;

    private static bool IsValidItemID(ulong id) => id != 0 && id < 0xF000000000000000;

    private bool TryGetLoadoutItem(Team team, ushort definitionIndex, out (Team team, loadout_slot_t slot) indices)
    {
        indices = default;
        for (var slot = 0; slot < (int)loadout_slot_t.LOADOUT_SLOT_COUNT; slot++)
        {
            if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
            {
                indices = (team, (loadout_slot_t)slot);
                return true;
            }
        }
        foreach (var (slot, itemView) in Natives.Instance.CCSInventoryManager.GetDefaultLoadouts(team))
        {
            if (itemView.ItemDefinitionIndex == definitionIndex)
            {
                indices = (team, slot);
                return true;
            }
        }
        return false;
    }

    public void UpdateLoadoutItem(Team team, ushort definitionIndex, ulong itemID)
    {
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            loadout.ItemId = itemID;
            loadout.DefinitionIndex = definitionIndex;
        }
    }

    /// <summary>
    /// Look up the loadout <see cref="CEconItemView"/> for a given team + definition
    /// index. This is the same view the native spawn code passes to GiveNamedItem on
    /// client (re)connect, so it carries the full CustomAttributeData (stickers,
    /// keychain, StatTrak, nametag) produced by <see cref="UpdateWeaponSkin"/> et al.
    /// </summary>
    public CEconItemView? TryGetLoadoutItemViewByDefIndex(Team team, ushort definitionIndex)
    {
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
            return GetItemInLoadout(indices.team, indices.slot);
        return null;
    }

    public bool TryGetItemID(Team team, ushort definitionIndex, out ulong itemID)
    {
        itemID = 0;
        if (TryGetLoadoutItem(team, definitionIndex, out var indices))
        {
            ref var loadout = ref Loadouts[indices.team, indices.slot];
            itemID = loadout.ItemId;
            return IsValidItemID(itemID);
        }
        return false;
    }

    public bool TryGetEconItemByItemID(ulong itemid, [MaybeNullWhen(false)] out CEconItem item)
    {
        var ptr = Natives.Instance.GetEconItemByItemID.Call(Address, itemid);
        if (ptr == 0) { item = null; return false; }
        item = new CEconItem(ptr);
        return true;
    }

    // ── SO events ───────────────────────────────────────────────
    public void SODestroyed(ulong steamid, CEconItem item) =>
        Natives.Instance.SODestroyed(this, new SOID_t(steamid), item);
    public void SOCreated(ulong steamid, CEconItem item) =>
        Natives.Instance.SOCreated(this, new SOID_t(steamid), item);
    public void SOUpdated(ulong steamid, CEconItem item) =>
        Natives.Instance.SOUpdated(this, new SOID_t(steamid), item);

    // ── Get default item IDs ────────────────────────────────────
    private ulong GetDefaultWeaponSkinItemID(Team team, ushort definitionIndex)
    {
        for (var slot = 0; slot < (int)loadout_slot_t.LOADOUT_SLOT_COUNT; slot++)
            if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
                return Loadouts[team, slot].ItemId;
        return 0;
    }
    private ulong GetDefaultKnifeSkinItemID(Team team) =>
        _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_MELEE].ItemId;
    private ulong GetDefaultGloveSkinItemID(Team team) =>
        _defaultLoadouts[team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS].ItemId;

    // ── Update inventory items ──────────────────────────────────
    //
    // Strategy: if the loadout already references a NON-default CEconItem we own
    // for this (team, defIndex / slot), mutate THAT item's CustomAttributeData in
    // place and fire SOUpdated. This keeps ItemID stable so the loadout's
    // CEconItemView continues to reference valid memory — necessary because
    // destroying and recreating the underlying item leaves the view's sticker
    // snapshot dangling / stale, which is why sticker edits on the same skin
    // silently failed to apply (only a brand-new skin worked).
    //
    // Only when there is no existing owned item for this slot do we create a
    // fresh CEconItem and add it to the SOCache.

    public void UpdateWeaponSkin(WeaponSkinData data)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var defaultId = GetDefaultWeaponSkinItemID(data.Team, data.DefinitionIndex);
            if (TryGetItemID(data.Team, data.DefinitionIndex, out var existingId) &&
                existingId != defaultId &&
                TryGetEconItemByItemID(existingId, out var existing))
            {
                // In-place mutation: preserves ItemID and the loadout CEconItemView's
                // backing pointer, so stickers/keychain refresh reliably.
                existing.Apply(data);
                SOUpdated(SteamID, existing);
                return;
            }

            var item = Natives.Instance.CreateCEconItemInstance();
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            UpdateLoadoutItem(data.Team, data.DefinitionIndex, item.ItemID);
            item.Apply(data);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public void UpdateKnifeSkin(KnifeSkinData data)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            ref var loadout = ref Loadouts[data.Team, loadout_slot_t.LOADOUT_SLOT_MELEE];
            var defaultId = GetDefaultKnifeSkinItemID(data.Team);

            if (IsValidItemID(loadout.ItemId) &&
                loadout.ItemId != defaultId &&
                TryGetEconItemByItemID(loadout.ItemId, out var existing))
            {
                // In-place mutation for sticker/nametag/StatTrak/paint edits. If the
                // knife *definition* itself changed, we also update the definition
                // index on the same item so the loadout stays consistent.
                existing.DefinitionIndex = data.DefinitionIndex;
                loadout.DefinitionIndex = data.DefinitionIndex;
                existing.Apply(data);
                SOUpdated(SteamID, existing);
                return;
            }

            var item = Natives.Instance.CreateCEconItemInstance();
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            loadout.ItemId = item.ItemID;
            loadout.DefinitionIndex = data.DefinitionIndex;
            item.Apply(data);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public void UpdateGloveSkin(GloveData data)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            ref var loadout = ref Loadouts[data.Team, loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS];
            var defaultId = GetDefaultGloveSkinItemID(data.Team);

            if (IsValidItemID(loadout.ItemId) &&
                loadout.ItemId != defaultId &&
                TryGetEconItemByItemID(loadout.ItemId, out var existing))
            {
                existing.DefinitionIndex = data.DefinitionIndex;
                loadout.DefinitionIndex = data.DefinitionIndex;
                existing.Apply(data);
                SOUpdated(SteamID, existing);
                return;
            }

            var item = Natives.Instance.CreateCEconItemInstance();
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            loadout.ItemId = item.ItemID;
            loadout.DefinitionIndex = data.DefinitionIndex;
            item.Apply(data);
            SOCache.AddObject(item);
            SOCreated(SteamID, item);
            SOUpdated(SteamID, item);
        });
    }

    public void UpdateMusicKit(int musicKitIndex)
    {
        Core.Scheduler.NextWorldUpdate(() =>
        {
            var item = Natives.Instance.CreateCEconItemInstance();
            for (int teamIndex = 0; teamIndex < 2; teamIndex++)
            {
                var team = (Team)teamIndex;
                ref var loadout = ref Loadouts[team, loadout_slot_t.LOADOUT_SLOT_MUSICKIT];
                if (teamIndex == 0)
                {
                    if (IsValidItemID(loadout.ItemId) && TryGetEconItemByItemID(loadout.ItemId, out var oldItem))
                    {
                        if (loadout.ItemId != _defaultLoadouts[(Team)0, loadout_slot_t.LOADOUT_SLOT_MUSICKIT].ItemId)
                            SOCache.RemoveObject(oldItem);
                        SODestroyed(SteamID, oldItem);
                    }
                    item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
                    item.ItemID = GetNewItemID();
                    item.InventoryPosition = GetNewInventoryPosition();
                    item.DefinitionIndex = 1314;
                    item.ConfigureAttributes(cd => cd.SetMusicId(musicKitIndex));
                    SOCache.AddObject(item);
                    SOCreated(SteamID, item);
                    SOUpdated(SteamID, item);
                }
                loadout.ItemId = item.ItemID;
                loadout.DefinitionIndex = 1314;
            }
        });
    }
}
