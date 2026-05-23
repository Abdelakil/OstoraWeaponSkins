# OstoraWeaponSkins Performance Audit — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix confirmed memory leaks and add diagnostic commands to OstoraWeaponSkins.

**Architecture:** Three independent fixes in two source files plus a new RCON command — no new files, no refactoring, no behavioral changes beyond leak fixes.

**Tech Stack:** C#, SwiftlyS2 (CS2 plugin framework), FreeSql

---

## File Map

| File | Changes |
|------|---------|
| `src/OstoraWeaponSkins.cs` | Fix 1: `_skinCache.TryRemove` in `SOCacheUnsubscribed`. Fix 3: new `ws_status` command + registration. Comment annotations on scheduler nesting. |
| `src/Natives/NativeStructs.cs` | Fix 2: `Core.Memory.Free(oldItem.pCustomData)` before each `SODestroyed(oldItem)` call. |

---

### Task 1: Fix `_skinCache` Memory Leak

**Files:**
- Modify: `src/OstoraWeaponSkins.cs:104-111`

- [ ] **Step 1: Add `_skinCache.TryRemove` to the unsubscribe handler**

Edit `src/OstoraWeaponSkins.cs`. In the `Native.OnSOCacheUnsubscribed` handler (line 104-111), add `_skinCache.TryRemove(soid.SteamID, out _);` after the existing `_defaultModels.TryRemove`:

```csharp
Native.OnSOCacheUnsubscribed += (_inv, soid) =>
{
    _subscribedSteamIds.TryRemove(soid.SteamID, out _);
    _loadEpochs.TryRemove(soid.SteamID, out _);
    _autoRefreshFlags.TryRemove(soid.SteamID, out _);
    _defaultModels.TryRemove(soid.SteamID, out _);
    _skinCache.TryRemove(soid.SteamID, out _);
    DebugLog("[OSTORA] SOCache unsubscribed for {SteamID}", soid.SteamID);
};
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build`
Expected: Build succeeds with no errors.

- [ ] **Step 3: Commit**

```bash
git add src/OstoraWeaponSkins.cs
git commit -m "fix: clean up _skinCache on player disconnect"
```

---

### Task 2: Fix CustomAttributeData Native Memory Leak

**Files:**
- Modify: `src/Natives/NativeStructs.cs:407-464` (methods `UpdateWeaponSkin`, `UpdateKnifeSkin`, `UpdateGloveSkin`)

- [ ] **Step 1: Add `Core.Memory.Free` before `SODestroyed` in `UpdateWeaponSkin`**

In `UpdateWeaponSkin` (line 421), before `SODestroyed(SteamID, oldItem);`:

```csharp
if (oldItem.pCustomData != 0)
    Core.Memory.Free(oldItem.pCustomData);
SODestroyed(SteamID, oldItem);
```

The full method after edit (lines 407-438):

```csharp
public void UpdateWeaponSkin(WeaponSkinData data)
{
    Core.Scheduler.NextWorldUpdate(() =>
    {
        var item = Natives.Instance.CreateCEconItemInstance();
        if (TryGetItemID(data.Team, data.DefinitionIndex, out var existingId) &&
            TryGetEconItemByItemID(existingId, out var oldItem))
        {
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();

            if (existingId != GetDefaultWeaponSkinItemID(data.Team, data.DefinitionIndex))
                SOCache.RemoveObject(oldItem);
            if (oldItem.pCustomData != 0)
                Core.Memory.Free(oldItem.pCustomData);
            SODestroyed(SteamID, oldItem);

            UpdateLoadoutItem(data.Team, data.DefinitionIndex, item.ItemID);
        }
        else
        {
            item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
            item.ItemID = GetNewItemID();
            item.InventoryPosition = GetNewInventoryPosition();
            UpdateLoadoutItem(data.Team, data.DefinitionIndex, item.ItemID);
        }

        item.Apply(data);
        SOCache.AddObject(item);
        SOCreated(SteamID, item);
        SOUpdated(SteamID, item);
    });
}
```

- [ ] **Step 2: Add `Core.Memory.Free` before `SODestroyed` in `UpdateKnifeSkin`**

In `UpdateKnifeSkin` (line 451), before `SODestroyed(SteamID, oldItem);`:

```csharp
if (oldItem.pCustomData != 0)
    Core.Memory.Free(oldItem.pCustomData);
SODestroyed(SteamID, oldItem);
```

The full relevant block (lines 447-452):

```csharp
if (IsValidItemID(loadout.ItemId) && TryGetEconItemByItemID(loadout.ItemId, out var oldItem))
{
    if (loadout.ItemId != GetDefaultKnifeSkinItemID(data.Team))
        SOCache.RemoveObject(oldItem);
    if (oldItem.pCustomData != 0)
        Core.Memory.Free(oldItem.pCustomData);
    SODestroyed(SteamID, oldItem);
}
```

- [ ] **Step 3: Add `Core.Memory.Free` before `SODestroyed` in `UpdateGloveSkin`**

Read `UpdateGloveSkin` from the file to confirm its exact lines, then add the same pattern. Find the `SODestroyed(SteamID, oldItem);` call and insert before it:

```csharp
if (oldItem.pCustomData != 0)
    Core.Memory.Free(oldItem.pCustomData);
SODestroyed(SteamID, oldItem);
```

- [ ] **Step 4: Build to verify compilation**

Run: `dotnet build`
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add src/Natives/NativeStructs.cs
git commit -m "fix: free native CustomAttributeData memory before destroying CEconItem"
```

---

### Task 3: Add `ws_status` Diagnostic Command

**Files:**
- Modify: `src/OstoraWeaponSkins.cs` (register command in `Load()`, add handler method)

- [ ] **Step 1: Register the `ws_status` command in `Load()`**

In `Load()`, after the `ws_debug` registration (line 126), add:

```csharp
// RCON command: ws_status — prints diagnostic info (cache sizes, memory usage)
Core.Command.RegisterCommand("ws_status", OnStatusCommand, registerRaw: true);
```

- [ ] **Step 2: Add the `OnStatusCommand` handler**

Add this method after `OnDebugCommand` (after line 413):

```csharp
private void OnStatusCommand(SwiftlyS2.Shared.Commands.ICommandContext context)
{
    var skinCacheCount = _skinCache.Count;
    var loadEpochCount = _loadEpochs.Count;
    var subscribedCount = _subscribedSteamIds.Count;
    var defaultModelsCount = _defaultModels.Sum(kvp => kvp.Value.Count);
    var managedMemory = GC.GetTotalMemory(false) / 1024;

    Logger.LogInformation("=== OSTORA WeaponSkins Status ===");
    Logger.LogInformation("Skin cache entries: {Count}", skinCacheCount);
    Logger.LogInformation("Load epochs: {Count}", loadEpochCount);
    Logger.LogInformation("Subscribed SteamIDs: {Count}", subscribedCount);
    Logger.LogInformation("Default model entries: {Count}", defaultModelsCount);
    Logger.LogInformation("Managed memory: {Memory} KB", managedMemory);
    Logger.LogInformation("================================");
}
```

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add src/OstoraWeaponSkins.cs
git commit -m "feat: add ws_status diagnostic command"
```

---

### Task 4: Add Nesting Comments to `LoadAndApplyPlayer`

**Files:**
- Modify: `src/OstoraWeaponSkins.cs:291-377`

- [ ] **Step 1: Annotate each scheduler nesting level**

The current nesting in `LoadAndApplyPlayer` (line 291 onward):

```
Level 1: NextWorldUpdate (line 291)  — runs the DB-loaded data apply on the game thread
Level 2: NextWorldUpdate (line 333)  — lets inventory SO events settle before re-giving weapons
Level 3: NextWorldUpdate (line 335)  — separates regive from the re-filter for team changes
DelayBySeconds(0.5) (line 365)       — delayed glove texture fix
```

Add comments:

At line 332 (`// Apply visuals after inventory updates settle (2 ticks)`):
```csharp
// Level 2: NextWorldUpdate — wait 1 tick for the inventory SO events (SOCreated/SOUpdated)
// from the Update*Skin calls above to propagate before touching weapon entities.
Core.Scheduler.NextWorldUpdate(() =>
{
    // Level 3: NextWorldUpdate — another tick for loadout state to stabilize after
    // re-filtering for team, so RegivePlayerWeaponsFromData sees the final loadout.
    Core.Scheduler.NextWorldUpdate(() =>
    {
```

And at line 364:
```csharp
// DelayBySeconds(0.5) — deferred glove texture fix: some glove models need extra
// time after the agent model swap before SetBodygroup takes effect.
Core.Scheduler.DelayBySeconds(0.5f, () =>
```

- [ ] **Step 2: Commit**

```bash
git add src/OstoraWeaponSkins.cs
git commit -m "docs: annotate scheduler nesting in LoadAndApplyPlayer"
```

---

## Manual Test Plan

1. Deploy plugin to a CS2 server.
2. Start server, check console for "OSTORA Loaded."
3. Run `ws_status` — all counts should be 0, memory baseline noted.
4. Connect a player. Run `ws_status`:
   - `_skinCache` = 1 (or number of teams the player has skins for)
   - `subscribedSteamIds` = 1
5. Disconnect player. Run `ws_status`:
   - `_skinCache` = 0 ✓ (Fix 1)
   - `subscribedSteamIds` = 0
6. Repeat join/leave 10x. Run `ws_status` each time — counts should not grow.
7. Play a match, verify weapon skins, gloves, knives, agents, and music apply correctly (Fix 2 must not break item creation).
8. Leave server running idle for 2+ hours. Join and check first-spawn weapon switch speed.
