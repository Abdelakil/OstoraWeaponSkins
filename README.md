# Ostora Weapon Skins

SwiftlyS2 plugin — applies player weapon skins, knives, gloves, agents, and music kits from a MySQL database in Counter-Strike 2.

## Architecture

```
                    ┌──────────────┐
                    │  database.jsonc  │  SwiftlyS2 global config
                    │ "OstoraWeaponskins" │
                    └──────┬───────┘
                           │ connection string by name
                           ▼
┌─────────────────────────────────────────────────────┐
│                   Plugin.cs                          │
│  OstoraWeaponSkins : BasePlugin                     │
│                                                     │
│  Load() → config, gamedata, data files, hooks       │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────┐ │
│  │ Event hooks │  │ ws_refreshskins │  │ DB Sync   │ │
│  │ (spawn,     │  │    command      │  │ (connect,  │ │
│  │  entity,    │  │                 │  │  disconnect│ │
│  │  round)     │  │  steamid64|all  │  │  )         │ │
│  └─────┬───────┘  └──────┬─────────┘  └─────┬──────┘ │
│        │                 │                   │        │
└────────┼─────────────────┼───────────────────┼────────┘
         │                 │                   │
         ▼                 ▼                   ▼
┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐
│  SkinApplier.cs │ │ Database.cs  │ │   WeaponInfo.cs  │
│                 │ │              │ │                  │
│ GivePlayerWeapon│ │ LoadPlayer   │ │  WeaponInfo      │
│ Skin            │ │ Data(IPlayer)│ │  StickerInfo     │
│ GivePlayerGloves│ │              │ │  KeyChainInfo    │
│ GivePlayerAgent │ │ wp_player_*  │ │  PlayerInfo      │
│ GivePlayerMusic │ │   tables     │ │                  │
│ RefreshWeapons  │ │              │ │                  │
└────────┬────────┘ └──────┬───────┘ └──────────────────┘
         │                 │
         ▼                 ▼
┌──────────────────────────────────────┐
│          CS2 Game Server             │
│  Entity attributes, schema types,    │
│  CEconItemView, CCSPlayerPawn, etc.  │
└──────────────────────────────────────┘
```

## Files

| File | Lines | Purpose |
|------|-------|---------|
| `Plugin.cs` | ~720 | Plugin entry, metadata, config loading, `ws_refreshskins` command, game events (spawn/round/mvp), framework events (connect/disconnect/entity), DB sync, weapon tables |
| `SkinApplier.cs` | ~280 | `GivePlayerWeaponSkin()` — sets paint/seed/wear via `SetOrAddAttribute`, knife subclass change, glove/agent/music application, weapon refresh (kill + re-give) |
| `Database.cs` | ~256 | MySQL queries via Dapper. Reads all `wp_player_*` tables. Sticker/keychain parsing. Team-aware load (2=T, 3=CT, 0=both). |
| `Config.cs` | 12 | `PluginConfig` model — DB connection name, language, feature toggles |
| `WeaponInfo.cs` | 38 | `WeaponInfo`, `StickerInfo`, `KeyChainInfo`, `PlayerInfo` data classes |

## Data flow

1. **Player connects** → `OnClientPutInServer` → `Database.LoadPlayerData(player)` → populates `GPlayerWeaponsInfo`, `GPlayersKnife`, `GPlayersGlove`, `GPlayersAgent`, `GPlayersMusic`
2. **Player spawns** → `OnPlayerSpawn` → `GivePlayerGloves`, `GivePlayerAgent`, `GivePlayerMusicKit`
3. **Weapon given** → `GivePlayerWeaponSkin` → reads player's cached `WeaponInfo` → sets `AttributeList` paint/seed/wear via `SetOrAddAttribute`
4. **`ws_refreshskins` command** → reloads all DB data → calls `RefreshWeapons` → kills and re-gives weapons (skin applied via the give hook)

## Command

```
ws_refreshskins <steamid64|all>
```

Server console only. No `sw_` prefix (registered raw).

## Configuration

`resources/config.json`:

```json
{
  "OstoraWeaponSkins": {
    "DatabaseConnection": "OstoraWeaponskins",
    "SkinsLanguage": "en",
    "CmdRefreshCooldownSeconds": 3,
    "KnifeEnabled": true,
    "SkinEnabled": true,
    "GloveEnabled": true,
    "AgentEnabled": true,
    "MusicEnabled": true
  }
}
```

Database credentials come from SwiftlyS2's `configs/database.jsonc` entry named `OstoraWeaponskins`.

## Database tables

| Table | Key columns |
|-------|-------------|
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

Place in the plugin's `data/` directory (at `Core.PluginPath/data/`):

- `skins_en.json` — weapon skin definitions (paint, defindex, paint_name)
- `gloves_en.json` — glove definitions
- `agents_en.json` — agent model definitions
- `music_en.json` — music kit definitions

Copied from the CS# WeaponPaints plugin's data files.
