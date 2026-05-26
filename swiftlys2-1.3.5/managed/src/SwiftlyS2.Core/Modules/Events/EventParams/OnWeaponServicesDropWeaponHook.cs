using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.Events;

internal class OnWeaponServicesDropWeaponHook : IOnWeaponServicesDropWeaponHook
{

    public required CCSPlayer_WeaponServices WeaponServices { get; set; }

    public required CBasePlayerWeapon? Weapon { get; set; }

    public required bool SwappingWeapon { get; set; }
    public required HookResult Result { get; set; } = HookResult.Continue;
}