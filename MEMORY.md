# MEMORY — OstoraWeaponSkins Porting Learnings

## Debugging Timeline

### Round 1: Build failures (63 errors → 0)
- All types in wrong namespaces. `SwiftlyPlayer` doesn't exist — real type is `IPlayer`.
- Correct namespaces discovered via DLL binary search + SwiftlyS2 1.3.5 source code inspection.
- `[GameEventHandler]` attribute pattern replaced with `Core.GameEvent.HookPost<T>()` registration.

### Round 2: DB connection (timeout error)
- `database.jsonc` has `timeout` field → MySqlConnector rejects `timeout=30` in connection string.
- Fix: regex strip before `MySqlConnectionStringBuilder`.

### Round 3: DB schema mismatch (agent_ct/agent_t)
- Our code queried CS# schema columns (`agent_ct`, `agent_t`). Server uses `weapon_team` + `agent_index`.
- Fix: rewritten query + agent model lookup via image URL (agents_en.json has no `weapon_defindex`).

### Round 4: `bit(1)` → `bool` crash
- `weapon_stattrak` is `bit(1)`, Dapper returns `byte` not `bool?`.
- Fix: `Convert.ToBoolean()`.

### Round 5: Skins invisible (multiple root causes)
- **A:** `SetOrAddAttribute` never called — only `FallbackPaintKit`/`FallbackSeed`/`FallbackWear` set, attributes left empty after `RemoveAll()`.
- **B:** Integer values passed as plain floats — `(float)421` = 421.0f. CS2 expects bit-reinterpreted: `BitConverter.Int32BitsToSingle(421)` ≈ 5.9e-43f.
- **C:** `OnEntitySpawned` → `NextWorldUpdate` → `OwnerEntity.Index` compared with `controller.Index` — never matches. `OwnerEntity` is the PAWN, not controller. Fix: `Core.PlayerManager.GetPlayerFromPawn()`.
- **D:** `RefreshWeapons` broken — `new CBasePlayerWeapon()` on abstract interface. Fix: apply `GivePlayerWeaponSkin` directly after `ItemServices.GiveItem()`.

### Round 6: Agent ERROR box
- Model path was `agents/characters/...` → CS2 uses `agents/models/...` (verified against CS# reference).
- `SetModel()` is `[ThreadUnsafe]`, throws if not on game thread. Fix: `SetModelAsync()`.

### Round 7: Knife no paint, gloves missing, agent reverts
- **Knife:** `return` after subclass change exited before paint code. CS# uses `break`, falls through.
- **Gloves:** Missing `player.ExecuteCommand("lastinv")` before/after — CS2 requires this to refresh glove model.
- **Agent:** CS2 loadout system resets model after initial set. Fix: re-apply with 0.3s delay.

### Round 8: Gloves still not rendering + no paint
- **Glove bodygroup toggle missing.** CS# calls `SetBodygroup(pawn, "first_or_third_person", 0)` then 0.2s later `...,1` after the second `lastinv`. Without this, glove model doesn't display. Fix: added bodygroup toggle inside the delay callback.
- **Silent catch block** in delay callback swallowed all exceptions — zero visibility. Fix: `catch (Exception ex) { Console.WriteLine(...) }`.
- **Diagnostic log added:** `"Gloves applied: defindex={gloveId} paint={paint} seed={seed}"` for verification.
- **Full CS# glove pattern:** clear attributes → lastinv → 0.08s delay → set defindex/IDs/paint/seed/wear → initialized=true → lastinv → bodygroup(0) → 0.2s delay → bodygroup(1).

### Round 9: Gloves only on CT (team data overwritten)
- **Dapper pattern matching trap.** MySQL `smallint(6)` returned as boxed `long` by Dapper/MySqlConnector. `row.weapon_team switch { 2 => 2, 3 => 3, _ => 0 }` — boxed `long(2)` does NOT match int constant `2` in C# pattern matching. ALL rows fell to `_ => 0`, storing both rows under both teams. Second row (CT, paint=1410) overwrote first row (T, paint=10048). Result: both teams showed CT paint.
- **Fix:** `int weaponTeam = (int)row.weapon_team;` — explicit cast unboxes correctly regardless of underlying type. Then plain `if` check instead of switch.
- **Other methods (GetKnife, GetGlove, GetMusic)** already had `(int)` cast before switch, so they were unaffected.

## Key API Mappings (CS# → SwiftlyS2)

### Plugin base
| CS# | SwiftlyS2 |
|-----|-----------|
| `BasePlugin` | `BasePlugin(ISwiftlyCore core)` with protected `Core` |
| `ModuleDirectory` | `Core.PluginPath` |
| `AddCommand(name, desc, handler)` | `Core.Command.RegisterCommand(name, handler, registerRaw, permission)` |
| `RegisterEventHandler<T>(handler)` | `Core.GameEvent.HookPost<T>(handler)` |
| `Server.NextFrame(action)` | `Core.Scheduler.NextTick(action)` |
| `AddTimer(seconds, action)` | `Core.Scheduler.DelayBySeconds(seconds, action)` |
| `Utilities.GetPlayers()` | `Core.PlayerManager.GetAllPlayers()` |

### Players
| CS# | SwiftlyS2 |
|-----|-----------|
| `CCSPlayerController` | `IPlayer` + `.Controller` |
| `player.IsBot` | `player.IsFakeClient` |
| `player.PlayerName` | `player.Name` |
| `player.PawnIsAlive` | `player.IsAlive` |
| `player.UserId` | `player.UserID` |
| `player.Team` (CsTeam enum) | `player.Controller.TeamNum` (byte → cast to `int`) |
| `player.GiveNamedItem(...)` | `.ItemServices?.GiveItem<T>(classname)` |
| `player.ExecuteClientCommand(...)` | `player.ExecuteCommand(...)` |
| `player.PrintToChat(msg)` | `player.SendChat(msg)` |

### Game events
| CS# | SwiftlyS2 |
|-----|-----------|
| `@event.Userid` | `@event.UserIdPlayer` (IPlayer?) |
| `@event.Attacker` | `@event.AttackerPlayer` |
| `+ GameEventInfo info` | No 2nd parameter |
| `RegisterEventHandler<T>(handler, HookMode.Post)` | `Core.GameEvent.HookPost<T>(handler)` |

### Entities
| CS# | SwiftlyS2 |
|-----|-----------|
| `new CBasePlayerWeapon(handle)` | `entity as CBasePlayerWeapon` or pattern match |
| `weapon.OwnerEntity.Index` → player controller | `PlayerManager.GetPlayerFromPawn(pawn)` |
| `CAttributeListSetOrAddAttributeValueByName.Invoke(handle, name, value)` | `attributeList.SetOrAddAttribute(name, value)` |
| `ViewAsFloat((uint)value)` | `BitConverter.Int32BitsToSingle(value)` |
| `Utilities.SetStateChanged(...)` | `.Updated()` or `.XxxUpdated()` |

## Namespace Cheat Sheet

| Namespace | Key Types |
|-----------|----------|
| `SwiftlyS2.Shared.Plugins` | BasePlugin, PluginMetadata |
| `SwiftlyS2.Shared.Players` | IPlayer, IPlayerManagerService |
| `SwiftlyS2.Shared.Commands` | ICommandService, ICommandContext, CommandAttribute |
| `SwiftlyS2.Shared.GameEvents` | IGameEventService, GameEventHandlerAttribute |
| `SwiftlyS2.Shared.GameEventDefinitions` | EventPlayerSpawn, EventRoundStart, etc. |
| `SwiftlyS2.Shared.SchemaDefinitions` | CBaseEntity, CCSPlayerController, CBasePlayerWeapon, CEconItemView, CAattributeList |
| `SwiftlyS2.Shared.Events` | IEventSubscriber |
| `SwiftlyS2.Shared.Database` | IDatabaseService |
| `SwiftlyS2.Shared.Misc` | HookResult (Continue=0, Stop=1), HookMode (Pre=0, Post=1) |
| `SwiftlyS2.Shared` | ISwiftlyCore |

## Process Lessons

1. **Always add diagnostics before guessing.** The `Console.WriteLine` at every layer (DB query, entity spawn, skin application) revealed exactly where data stopped flowing.
2. **Study the reference plugin line-by-line.** Every bug was fixed by comparing our code against the CS# original — `lastinv` calls, agent model path, knife `break` vs `return`, delayed glove application pattern.
3. **Schema interfaces vs. wrapper classes.** CS# wraps entities; SwiftlyS2 uses schema interfaces (`ISchemaClass<T>`). Can't `new` them, can't assume nullable reference semantics.
4. **Thread safety.** Every entity mutation method in SwiftlyS2 is `[ThreadUnsafe]`. Use async variants or ensure game thread context.
5. **Two different float encodings.** Paint/seed use regular `int→float` (`(float)421` = `421.0f`). StatTrak count, sticker IDs, and keychain IDs use `IntAsFloat` bit-reinterpretation (`BitConverter.Int32BitsToSingle(5)` = `7e-45f`). Using `IntAsFloat` on paint/seed causes near-zero values → invisible skins. Discovered by comparing CS# reference code: weapon section passes `FallbackPaintKit` directly (int→float), StatTrak section uses `ViewAsFloat((uint)count)`. Glove section also passes paint/seed directly.
