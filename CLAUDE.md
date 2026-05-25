# OstoraWeaponSkins

SwiftlyS2 plugin that reads player weapon skins, knives, gloves, agents, and music kits from MySQL and applies them in CS2 via the `ws_refreshskins` command.

## Architecture

The plugin class (`OstoraWeaponSkins`) is a `partial class` split across 3 files:

| File | Role |
|------|------|
| `Plugin.cs` | `BasePlugin` subclass. `[PluginMetadata]`, `Load()`, config (SwiftlyS2 standard `configs/plugins/OstoraWeaponSkins/config.json`), gamedata, command, game events, entity spawn hooks, player state dictionaries |
| `SkinApplier.cs` | Partial of `OstoraWeaponSkins`. Game-level CS2 entity manipulation — weapon skins, gloves, knives, agents, music kits |
| `Database.cs` | `Database` class wrapping `MySqlConnection`. Dapper queries against `wp_player_*` tables |
| `Config.cs` | `PluginConfig` model — DB connection name, feature toggles |
| `WeaponInfo.cs` | `WeaponInfo`, `StickerInfo`, `KeyChainInfo`, `PlayerInfo` data classes |

## Build

```bash
dotnet build OstoraWeaponSkins.csproj
# Output: build/net10.0/OstoraWeaponSkins.dll
```

.NET 10. SwiftlyS2.CS2 via NuGet with `ExcludeAssets=runtime` + `PrivateAssets=all`.

## Command

`ws_refreshskins <steamid64|all>` — registered raw (no `sw_` prefix). Loads all player skin/knife/glove/agent/music data from MySQL and applies in-game.

## Critical API Patterns (SwiftlyS2 1.3.5/1.4)

### Entity types are schema INTERFACES, not classes
- NEVER `new` an entity type. Use `entity is CEconEntity econ` pattern matching, `.As<K>()` casting, or `CBasePlayerWeapon.From(handle)`.
- `CAttributeList.SetOrAddAttribute(string name, float value)` — direct method, no function pointers needed.

### Float encoding — TWO different types
**Regular float (int→float):** Paint kit and seed use standard C# int→float conversion.
```csharp
SetOrAddAttribute("set item texture prefab", (float)paintKit); // 421 → 421.0f
```
**Bit-reinterpreted (IntAsFloat):** StatTrak count, sticker IDs, and keychain IDs use ViewAsFloat-style.
```csharp
private static float IntAsFloat(int value) => BitConverter.Int32BitsToSingle(value);
SetOrAddAttribute("kill eater", IntAsFloat(killCount)); // 5 → 7e-45f (bits 0x00000005)
```
Applies to: `kill eater`, `sticker slot N id`, `keychain slot 0 id`, `keychain slot 0 seed`.
Does NOT apply to: `set item texture prefab` (use `(float)paint`), `set item texture seed` (use `(float)seed`).

### Player lookup from weapon entity
`weapon.OwnerEntity` points to the **pawn** (index 65+), NOT the controller (1-64). Use:
```csharp
var player = Core.PlayerManager.GetPlayerFromPawn(pawn);
```

### Thread safety on entity methods
`SetModel`, `AcceptInput`, `AddEntityIOEvent` are all `[ThreadUnsafe]`. Use async variants (`SetModelAsync`) or ensure called from game thread (event handlers).

### Agent model path
Models are at `agents/models/{model}.vmdl` (NOT `agents/characters/`). CS2 agent loadout system resets model after spawn — re-apply with 0.3s delay.

### Gloves — full application pattern
Matches CS# WeaponAction.cs lines 380-434 exactly:
1. Clear `EconGloves` attributes immediately
2. `player.ExecuteCommand("lastinv")` — force model refresh
3. After 0.08s delay: set `ItemDefinitionIndex`, paint/seed/wear via `IntAsFloat()` + `SetOrAddAttribute`, `Initialized = true`
4. `player.ExecuteCommand("lastinv")` again — apply model
5. **Bodygroup toggle**: `AcceptInput("SetBodygroup", "first_or_third_person,0")` then 0.2s later `...,1`
6. Log "Gloves applied" for diagnostics. Never silent-catch — log all exceptions.

### Knife skin flow
After knife subclass change (`AcceptInput("ChangeSubclass")`) and attribute clearing, **fall through** to paint application — do NOT `return` early. The CS# reference uses `break` in a switch, not `return`.

### Team values
`int`: 2 = T, 3 = CT, 0 = both/None, 1 = Spectator.

### IPlayer key members
`IsFakeClient` (bot), `IsAlive`, `SteamID` (ulong), `Slot`, `Name`, `Controller` (CCSPlayerController), `PlayerPawn` (CCSPlayerPawn?), `ExecuteCommand(string)`, `SendChat(string)`

### Dapper + switch pattern matching trap
Dapper returns MySQL `smallint` as boxed `long`/`short`. C# pattern matching on boxed values fails type check:
```csharp
// BROKEN: (object)(long)2 does NOT match pattern 2 (int)
int team = row.weapon_team switch { 2 => 2, 3 => 3, _ => 0 };
// All rows → _ => 0 (both teams), last write wins
```
**Always cast first:**
```csharp
int team = (int)row.weapon_team; // cast unboxes correctly
```

### Game events
Typed interfaces (`EventPlayerSpawn`) with `UserIdPlayer` → `IPlayer?`. Hook via:
```csharp
Core.GameEvent.HookPost<EventPlayerSpawn>(OnPlayerSpawn);
```

### Agent JSON lookup
`agents_en.json` has NO `weapon_defindex` field. The numeric ID is embedded in the `image` URL (e.g., `agent-4732.png`).
**Do NOT parse the image URL on every query.** Instead, build a lookup dictionary once at load time:
```csharp
// At load time:
foreach (var agent in AgentsList) {
    var match = Regex.Match(img, @"agent-(\d+)\.png");
    if (match.Success) AgentIndexLookup[idx] = agent;
}
// At query time: O(1) lookup
AgentIndexLookup.TryGetValue(agentIndex, out var agent);
```
CS# reference uses `agent_name` + `team` for menu selection, but server stores `agent_index`.

## Out of scope
No chat menus, no web UI, no stattrak toggle, no write-back to DB, no skin images on screen.
