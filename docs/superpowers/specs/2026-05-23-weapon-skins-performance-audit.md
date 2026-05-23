# OstoraWeaponSkins Performance Audit

Date: 2026-05-23

## Problem

After hours of server runtime, weapon switch animations become slow (taking seconds).
The issue occurs even on servers that sit empty for days — the first spawn after idle
is already affected.

## Root Causes Identified

### Confirmed Bugs

1. **`_skinCache` memory leak** — Entries added in `LoadAndApplyPlayer()` and
   `OnRefreshCommand()` but never removed on disconnect. The `SOCacheUnsubscribed`
   handler cleans up `_loadEpochs`, `_autoRefreshFlags`, and `_defaultModels` but
   omits `_skinCache`. Over 60-80 players per session, ~80 extra dictionary entries
   each holding 4x `List<T>` accumulate.

2. **CustomAttributeData native memory leak** — Each `UpdateWeaponSkin` /
   `UpdateKnifeSkin` / `UpdateGloveSkin` call creates a new `CEconItem` via
   `CreateCEconItemInstance()`. The `Apply()` → `ConfigureAttributes()` path calls
   `Core.Memory.Alloc` / `Core.Memory.Resize` for `pCustomData` — native heap
   allocations. When `SODestroyed` fires on the old item, its `pCustomData`
   native allocation is never freed. Over hours of play with spawns/refreshes/
   reconnects, this leaks native memory on the game heap.

## Fix Plan

### Fix 1: `_skinCache` Cleanup on Disconnect

- **File:** `src/OstoraWeaponSkins.cs`
- **Location:** `SOCacheUnsubscribed` handler (~line 104-111)
- **Change:** Add `_skinCache.TryRemove(soid.SteamID, out _);`

### Fix 2: Free Native CustomAttributeData on Item Destroy

- **File:** `src/Natives/NativeStructs.cs`
- **Location:** `UpdateWeaponSkin()`, `UpdateKnifeSkin()`, `UpdateGloveSkin()`
- **Change:** Before each `SODestroyed(oldItem)`, free `oldItem.pCustomData`:
  ```csharp
  if (oldItem.pCustomData != 0)
      Core.Memory.Free(oldItem.pCustomData);
  ```
- **API:** `Core.Memory.Free(nint pointer)` is available in SwiftlyS2 API
- **Access:** `CCSPlayerInventory` already has `[SwiftlyInject] private static ISwiftlyCore Core`

### Fix 3: Diagnostic RCON Commands

- **File:** `src/OstoraWeaponSkins.cs`
- **New command `ws_status`:** Reports `_skinCache` size, `_loadEpochs` size,
  `_subscribedSteamIds` count, `_defaultModels` size, managed memory usage,
  plugin uptime.
- **New command `ws_scheduler`:** If the scheduler exposes queue depth, show it;
  otherwise skip.

### Fix 4 (Deferred): Reduce Scheduler Nesting

- **Current state:** Triple-nested `NextWorldUpdate` + `DelayBySeconds(0.5)` in
  `LoadAndApplyPlayer` and `OnRefreshCommand`.
- **Decision:** Deferred. Nesting exists to let inventory writes settle before
  re-giving weapons for sticker/keychain/StatTrak networking. Not modifying
  without understanding the full interaction.
- **Action:** Add comments documenting why each nesting level exists.

## Testing

1. Load plugin on a test server. Run `ws_status` at start — note baseline values.
2. Connect as a player, spawn, play a round.
3. Run `ws_status` again — `_skinCache` should have 1 entry.
4. Disconnect. Run `ws_status` — `_skinCache` should be 0 (Fix 1).
5. Reconnect repeatedly (simulating hours of joins/leaves). Run `ws_status` —
   `_skinCache` should not grow unbounded.
6. Long-idle test: leave server running with no players for 2+ hours, then join
   and check first-spawn weapon switch speed.
