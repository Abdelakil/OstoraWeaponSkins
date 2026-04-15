using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace OstoraWeaponSkins;

public class Natives
{
    public static Natives Instance { get; private set; } = null!;

    private ISwiftlyCore Core { get; }
    private ILogger Logger { get; }
    private bool IsWindows => OperatingSystem.IsWindows();

    // ── Delegates ───────────────────────────────────────────────
    public unsafe delegate nint CreateCEconItemDelegate();
    public unsafe delegate byte AddObjectDelegate(nint pSOCache, nint pSharedObject);
    public unsafe delegate byte RemoveObjectDelegate(nint pSOCache, nint pSharedObject);
    public unsafe delegate void SOCreatedDelegate(nint pInventory, SOID_t* pSOID, nint pSharedObj, int eventType);
    public unsafe delegate void SOCreatedDelegate_Linux(nint pInventory, ulong soid1, ulong soid2, nint pSharedObj, int eventType);
    public unsafe delegate void SOUpdatedDelegate(nint pInventory, SOID_t* pSOID, nint pSharedObj, int eventType);
    public unsafe delegate void SOUpdatedDelegate_Linux(nint pInventory, ulong soid1, ulong soid2, nint pSharedObj, int eventType);
    public unsafe delegate void SODestroyedDelegate(nint pInventory, SOID_t* pSOID, nint pSharedObj, int eventType);
    public unsafe delegate void SODestroyedDelegate_Linux(nint pInventory, ulong soid1, ulong soid2, nint pSharedObj, int eventType);
    public unsafe delegate nint SOCacheSubscribedDelegate(nint pInventory, SOID_t* pSOID, nint pSOCache);
    public unsafe delegate nint SOCacheSubscribedDelegate_Linux(nint pInventory, ulong soid1, ulong soid2, nint pSOCache);
    public unsafe delegate nint SOCacheUnsubscribedDelegate(nint pInventory, SOID_t* pSOID);
    public unsafe delegate nint SOCacheUnsubscribedDelegate_Linux(nint pInventory, ulong soid1, ulong soid2);
    public delegate nint GetEconItemByItemIDDelegate(nint pInventory, ulong itemid);
    public delegate nint GetItemInLoadoutDelegate(nint pInventory, int team, int slot);
    public delegate nint UpdateItemViewDelegate(nint itemView, nint unk);
    public delegate nint CAttribute_String_NewDelegate(nint pAttributeString, nint pArena);
    public unsafe delegate nint GiveNamedItemDelegate(nint pItemServices, nint pItemName, nint subtype, nint pEconItemView, nint a5, nint a6);

    // ── Function pointers ───────────────────────────────────────
    public IUnmanagedFunction<CreateCEconItemDelegate> CreateCEconItem { get; }
    public IUnmanagedFunction<AddObjectDelegate> SOCache_AddObject { get; }
    public IUnmanagedFunction<RemoveObjectDelegate> SOCache_RemoveObject { get; }
    public IUnmanagedFunction<SOCreatedDelegate> CPlayerInventory_SOCreated { get; } = null!;
    public IUnmanagedFunction<SOCreatedDelegate_Linux> CPlayerInventory_SOCreated_Linux { get; } = null!;
    public IUnmanagedFunction<SOUpdatedDelegate> CPlayerInventory_SOUpdated { get; } = null!;
    public IUnmanagedFunction<SOUpdatedDelegate_Linux> CPlayerInventory_SOUpdated_Linux { get; } = null!;
    public IUnmanagedFunction<SODestroyedDelegate> CPlayerInventory_SODestroyed { get; } = null!;
    public IUnmanagedFunction<SODestroyedDelegate_Linux> CPlayerInventory_SODestroyed_Linux { get; } = null!;
    public IUnmanagedFunction<SOCacheSubscribedDelegate> CPlayerInventory_SOCacheSubscribed { get; } = null!;
    public IUnmanagedFunction<SOCacheSubscribedDelegate_Linux> CPlayerInventory_SOCacheSubscribed_Linux { get; } = null!;
    public IUnmanagedFunction<SOCacheUnsubscribedDelegate> CPlayerInventory_SOCacheUnsubscribed { get; } = null!;
    public IUnmanagedFunction<SOCacheUnsubscribedDelegate_Linux> CPlayerInventory_SOCacheUnsubscribed_Linux { get; } = null!;
    public IUnmanagedFunction<GetItemInLoadoutDelegate> CPlayerInventory_GetItemInLoadout { get; }
    public IUnmanagedFunction<GetEconItemByItemIDDelegate> GetEconItemByItemID { get; }
    public IUnmanagedFunction<CAttribute_String_NewDelegate> CAttribute_String_New { get; }
    public IUnmanagedFunction<UpdateItemViewDelegate> UpdateItemView { get; }
    public IUnmanagedFunction<GiveNamedItemDelegate>? GiveNamedItem { get; }

    // ── Offsets ─────────────────────────────────────────────────
    public int CCSPlayerInventory_LoadoutsOffset { get; }
    public int CCSInventoryManager_m_DefaultLoadoutsOffset { get; }
    public int CGCClientSharedObjectCache_m_OwnerOffset { get; }
    public int CCSPlayerInventory_m_pSOCacheOffset { get; }
    public int CCSPlayerInventory_m_ItemsOffset { get; }
    public int CCSPlayerController_InventoryServices_m_pInventoryOffset { get; }
    public CCSInventoryManager CCSInventoryManager { get; }

    // ── Events ──────────────────────────────────────────────────
    public event Action<CCSPlayerInventory, SOID_t>? OnSOCacheSubscribed;
    public event Action<CCSPlayerInventory, SOID_t>? OnSOCacheUnsubscribed;
    public event Action<CCSPlayer_ItemServices, CBasePlayerWeapon>? OnGiveNamedItemPost;

    public Natives(ISwiftlyCore core, ILogger logger)
    {
        Core = core;
        Logger = logger;

        var soCacheVtable = Core.Memory.GetVTableAddress("server", "GCSDK::CGCClientSharedObjectCache")!.Value;
        SOCache_AddObject = Core.Memory.GetUnmanagedFunctionByVTable<AddObjectDelegate>(soCacheVtable, Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::AddObject"));
        SOCache_RemoveObject = Core.Memory.GetUnmanagedFunctionByVTable<RemoveObjectDelegate>(soCacheVtable, Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::RemoveObject"));

        var playerInventoryVtable = Core.Memory.GetVTableAddress("server", "CCSPlayerInventory")!.Value;

        if (IsWindows)
        {
            CPlayerInventory_SOCreated = Core.Memory.GetUnmanagedFunctionByVTable<SOCreatedDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCreated"));
            CPlayerInventory_SOUpdated = Core.Memory.GetUnmanagedFunctionByVTable<SOUpdatedDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOUpdated"));
            CPlayerInventory_SODestroyed = Core.Memory.GetUnmanagedFunctionByVTable<SODestroyedDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SODestroyed"));
            CPlayerInventory_SOCacheSubscribed = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheSubscribedDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCacheSubscribed"));
            CPlayerInventory_SOCacheUnsubscribed = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheUnsubscribedDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCacheUnsubscribed"));
        }
        else
        {
            CPlayerInventory_SOCreated_Linux = Core.Memory.GetUnmanagedFunctionByVTable<SOCreatedDelegate_Linux>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCreated"));
            CPlayerInventory_SOUpdated_Linux = Core.Memory.GetUnmanagedFunctionByVTable<SOUpdatedDelegate_Linux>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOUpdated"));
            CPlayerInventory_SODestroyed_Linux = Core.Memory.GetUnmanagedFunctionByVTable<SODestroyedDelegate_Linux>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SODestroyed"));
            CPlayerInventory_SOCacheSubscribed_Linux = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheSubscribedDelegate_Linux>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCacheSubscribed"));
            CPlayerInventory_SOCacheUnsubscribed_Linux = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheUnsubscribedDelegate_Linux>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::SOCacheUnsubscribed"));
        }

        CPlayerInventory_GetItemInLoadout = Core.Memory.GetUnmanagedFunctionByVTable<GetItemInLoadoutDelegate>(playerInventoryVtable, Core.GameData.GetOffset("CPlayerInventory::GetItemInLoadout"));

        var stringVtable = Core.Memory.GetVTableAddress("server", "CAttribute_String")!.Value;
        CAttribute_String_New = Core.Memory.GetUnmanagedFunctionByVTable<CAttribute_String_NewDelegate>(stringVtable, Core.GameData.GetOffset("CAttribute_String::New"));

        CreateCEconItem = Core.Memory.GetUnmanagedFunctionByAddress<CreateCEconItemDelegate>(Core.GameData.GetSignature("CreateCEconItem"));
        GetEconItemByItemID = Core.Memory.GetUnmanagedFunctionByAddress<GetEconItemByItemIDDelegate>(Core.GameData.GetSignature("GetEconItemByItemID"));
        UpdateItemView = Core.Memory.GetUnmanagedFunctionByAddress<UpdateItemViewDelegate>(Core.GameData.GetSignature("UpdateItemView"));

        try { GiveNamedItem = Core.Memory.GetUnmanagedFunctionByAddress<GiveNamedItemDelegate>(Core.GameData.GetSignature("GiveNamedItem")); }
        catch { GiveNamedItem = null; }

        CCSPlayerInventory_LoadoutsOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_Loadouts");
        CCSInventoryManager_m_DefaultLoadoutsOffset = Core.GameData.GetOffset("CCSInventoryManager::m_DefaultLoadouts");
        CCSPlayerInventory_m_ItemsOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_Items");
        CCSPlayerInventory_m_pSOCacheOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_pSOCache");
        CCSPlayerController_InventoryServices_m_pInventoryOffset = Core.GameData.GetOffset("CCSPlayerController_InventoryServices::m_pInventory");
        CGCClientSharedObjectCache_m_OwnerOffset = Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::m_Owner");

        var xref = Core.GameData.GetSignature("CCSInventoryManager_xref");
        CCSInventoryManager = new CCSInventoryManager(Core.Memory.ResolveXrefAddress(xref)!);

        // Hook SOCacheSubscribed/Unsubscribed
        if (IsWindows)
        {
            CPlayerInventory_SOCacheSubscribed.AddHook(next =>
            {
                unsafe
                {
                    return (pInventory, pSOID, pSOCache) =>
                    {
                        try
                        {
                            var ret = next()(pInventory, pSOID, pSOCache);
                            OnSOCacheSubscribed?.Invoke(new CCSPlayerInventory(pInventory), *pSOID);
                            return ret;
                        }
                        catch (Exception e) { Logger.LogError(e, "SOCacheSubscribed error"); return 0; }
                    };
                }
            });
            CPlayerInventory_SOCacheUnsubscribed.AddHook(next =>
            {
                unsafe
                {
                    return (pInventory, pSOID) =>
                    {
                        try
                        {
                            var ret = next()(pInventory, pSOID);
                            OnSOCacheUnsubscribed?.Invoke(new CCSPlayerInventory(pInventory), *pSOID);
                            return ret;
                        }
                        catch (Exception e) { Logger.LogError(e, "SOCacheUnsubscribed error"); return 0; }
                    };
                }
            });
        }
        else
        {
            CPlayerInventory_SOCacheSubscribed_Linux.AddHook(next =>
            {
                return (pInventory, soid1, soid2, pSOCache) =>
                {
                    try
                    {
                        var ret = next()(pInventory, soid1, soid2, pSOCache);
                        OnSOCacheSubscribed?.Invoke(new CCSPlayerInventory(pInventory), new SOID_t(soid1, soid2));
                        return ret;
                    }
                    catch (Exception e) { Logger.LogError(e, "SOCacheSubscribed error"); return 0; }
                };
            });
            CPlayerInventory_SOCacheUnsubscribed_Linux.AddHook(next =>
            {
                return (pInventory, soid1, soid2) =>
                {
                    var ret = next()(pInventory, soid1, soid2);
                    OnSOCacheUnsubscribed?.Invoke(new CCSPlayerInventory(pInventory), new SOID_t(soid1, soid2));
                    return ret;
                };
            });
        }

        // Hook GiveNamedItem
        if (GiveNamedItem != null)
        {
            GiveNamedItem.AddHook(next =>
            {
                return (pItemServices, pItemName, subtype, pEconItemView, a5, a6) =>
                {
                    nint ret = 0;
                    try
                    {
                        ret = next()(pItemServices, pItemName, subtype, pEconItemView, a5, a6);
                        if (ret != 0)
                        {
                            var services = Helper.AsSchema<CCSPlayer_ItemServices>(pItemServices);
                            var weapon = Helper.AsSchema<CBasePlayerWeapon>(ret);
                            if (services.IsValid && weapon.IsValid)
                                OnGiveNamedItemPost?.Invoke(services, weapon);
                        }
                    }
                    catch (Exception e) { Logger.LogError(e, "GiveNamedItemPost error"); }
                    return ret;
                };
            });
        }

        Instance = this;
    }

    public CEconItem CreateCEconItemInstance() => new(CreateCEconItem.Call());

    public CAttribute_String CreateAttributeString()
    {
        var ret = CAttribute_String_New.Call(0, 0);
        return Helper.AsProtobuf<CAttribute_String>(ret, false);
    }

    public void SOCreated(CCSPlayerInventory inventory, SOID_t soid, CEconItem item)
    {
        unsafe
        {
            if (IsWindows) CPlayerInventory_SOCreated.CallOriginal(inventory.Address, &soid, item.Address, 4);
            else CPlayerInventory_SOCreated_Linux.CallOriginal(inventory.Address, soid.Part1, soid.Part2, item.Address, 4);
        }
    }

    public void SOUpdated(CCSPlayerInventory inventory, SOID_t soid, CEconItem item)
    {
        unsafe
        {
            if (IsWindows) CPlayerInventory_SOUpdated.CallOriginal(inventory.Address, &soid, item.Address, 4);
            else CPlayerInventory_SOUpdated_Linux.CallOriginal(inventory.Address, soid.Part1, soid.Part2, item.Address, 4);
        }
    }

    public void SODestroyed(CCSPlayerInventory inventory, SOID_t soid, CEconItem item)
    {
        unsafe
        {
            if (IsWindows) CPlayerInventory_SODestroyed.CallOriginal(inventory.Address, &soid, item.Address, 4);
            else CPlayerInventory_SODestroyed_Linux.CallOriginal(inventory.Address, soid.Part1, soid.Part2, item.Address, 4);
        }
    }

    public bool TryInitializeInventory(nint inventoryServicesAddress, out CCSPlayerInventory? inventory)
    {
        inventory = new CCSPlayerInventory(inventoryServicesAddress + CCSPlayerController_InventoryServices_m_pInventoryOffset);
        if (!inventory.IsValid) { inventory = null; return false; }
        return true;
    }
}
