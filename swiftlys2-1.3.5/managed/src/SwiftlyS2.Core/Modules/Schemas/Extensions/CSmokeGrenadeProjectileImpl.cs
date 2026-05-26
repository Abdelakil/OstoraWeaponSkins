using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.Scheduler;
using SwiftlyS2.Core.Services;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CSmokeGrenadeProjectileImpl : CSmokeGrenadeProjectile
{
    public static CSmokeGrenadeProjectile EmitGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
    {
        NativeBinding.ThrowIfNonMainThread();
        return new CSmokeGrenadeProjectileImpl(GameFunctions.CSmokeGrenadeProjectile_EmitGrenade(pos, angle, velocity,
            owner?.Address ?? nint.Zero, team,
            (uint)HelpersService.WeaponItemDefinitionIndices["weapon_smokegrenade"]));
    }

    public static Task<CSmokeGrenadeProjectile> EmitGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
    {
        return SchedulerManager.QueueOrNow(() => EmitGrenade(pos, angle, velocity, team, owner));
    }
}