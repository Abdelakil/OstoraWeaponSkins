# OstoraWeaponSkins

SwiftlyS2 plugin that reads player weapon skins, knives, gloves, agents, and music kits from MySQL and applies them in CS2.

## Command

```
ws_refreshskins <steamid64|all>
```

Server console only. No `sw_` prefix (registered raw).

## Configuration

Auto-generated on first load at `configs/plugins/ostoraweaponskins/config.json`

```json
{
  "OstoraWeaponSkins": {
    "DatabaseConnection": "OstoraWeaponskins",
    "SkinsLanguage": "en",
    "CmdRefreshCooldownSeconds": 3,
    "SkinEnabled": true,
    "KnifeEnabled": true,
    "GloveEnabled": true,
    "AgentEnabled": true,
    "MusicEnabled": true,
    "DebugLogging": false
  }
}
```

Database credentials come from SwiftlyS2's `configs/database.jsonc` using the connection name `OstoraWeaponskins`.

## Files

| File | Lines | Role |
|------|-------|------|
| `Plugin.cs` | ~720 | Plugin entry, config loading, command, game events, DB sync |
| `SkinApplier.cs` | ~280 | Weapon skin/knife/glove/agent/music application, weapon refresh |
| `Database.cs` | ~256 | MySQL queries via Dapper for all `wp_player_*` tables |
| `Config.cs` | ~38 | `PluginConfig` model |
| `WeaponInfo.cs` | ~38 | `WeaponInfo`, `StickerInfo`, `KeyChainInfo` data classes |

## Database tables

| Table | Columns |
|-------|---------|
| `wp_player_skins` | steamid, weapon_team, weapon_defindex, weapon_paint_id, weapon_wear, weapon_seed, weapon_nametag, weapon_stattrak, weapon_stattrak_count, weapon_sticker_[0-5], weapon_keychain |
| `wp_player_knife` | steamid, weapon_team, knife |
| `wp_player_gloves` | steamid, weapon_team, weapon_defindex |
| `wp_player_agents` | steamid, weapon_team, agent_index |
| `wp_player_music` | steamid, weapon_team, music_id |

Team values: 2 = Terrorist, 3 = Counter-Terrorist, 0 = both.

## Dependencies

- .NET 10
- SwiftlyS2.CS2 (NuGet, auto-resolved)
- MySqlConnector 2.5.0
- Dapper 2.1.72
- Newtonsoft.Json 13.0.3

## Build

```bash
dotnet build OstoraWeaponSkins.csproj
# Output: build/net10.0/OstoraWeaponSkins.dll
```

## Data files

Place in plugin's `data/` directory (at `Core.PluginPath/data/`):

- `skins_en.json` — weapon skin definitions
- `gloves_en.json` — glove definitions
- `agents_en.json` — agent model definitions
- `music_en.json` — music kit definitions

Copied from the CS# WeaponPaints plugin's data files.
