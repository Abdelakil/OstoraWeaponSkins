# OstoraWeaponSkins

A CS2 (Counter-Strike 2) plugin built with SwiftlyS2 that allows server administrators to apply custom weapon skins, knives, gloves, music kits, stickers, and keychains to players based on database configuration.

## Features

- **Weapon Skins**: Apply custom paint kits to weapons with floats, patterns, and nametags
- **Knife Skins**: Custom knife skins with full attribute support
- **Gloves**: Custom glove skins with wear and pattern support
- **Music Kits**: Player music kit customization
- **Stickers**: Apply up to 5 stickers per weapon with wear and rotation
- **Keychains**: Add keychains to weapons
- **StatTrak™**: StatTrak counter support
- **Runtime Control**: Debug logging toggle via RCON command
- **Database Support**: SQLite, MySQL, and PostgreSQL via FreeSql

## Installation

1. Build the project: `dotnet build --configuration Release`
2. Copy the `build/` directory contents to your CS2 server's SwiftlyS2 plugins directory
3. Configure your database connection in the plugin settings
4. Restart your CS2 server

## RCON Commands

- `ws_refreshskins <steamid>` - Refresh skins for a specific player
- `ws_debug <0|1>` - Toggle debug logging (0 = off, 1 = on)
- `ws_wipeskins <steamid>` - Remove all custom skins for a player

## Database Schema

The plugin uses FreeSql with the following tables:
- `WeaponSkinData` - Weapon skin configurations
- `KnifeSkinData` - Knife skin configurations
- `GloveData` - Glove configurations
- `MusicKitData` - Music kit configurations
- `StickerData` - Sticker configurations
- `KeychainData` - Keychain configurations

## Architecture

### Cross-Player Contamination Prevention
The plugin uses epoch-based staleness detection to prevent database responses from being applied to the wrong player. All inventory operations are tied to the correct player by re-deriving inventory from live player controllers.

### Sticker Application Fix
Skins with stickers are now applied correctly without requiring a server reconnect. The plugin:
- Uses in-place mutation of `CEconItem` to preserve `ItemID` stability
- Passes the correct `CEconItemView` to `GiveNamedItem` during weapon regive
- Fires `SOUpdated` events to propagate changes to the game client

## Building

```bash
dotnet build --configuration Release
```

## Requirements

- .NET 10.0
- SwiftlyS2.CS2 1.1.4
- CS2 server with SwiftlyS2 framework

## License

See LICENSE file for details.

## Contributing

Contributions are welcome! Please submit pull requests to the main branch.
