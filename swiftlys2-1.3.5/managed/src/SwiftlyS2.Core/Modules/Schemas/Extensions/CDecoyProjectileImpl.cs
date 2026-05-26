using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.Scheduler;
using SwiftlyS2.Core.Services;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CDecoyProjectileImpl : CDecoyProjectile
{
    public static CDecoyProjectile EmitGrenade( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner )
    {
        NativeBinding.ThrowIfNonMainThread();
        return new CDecoyProjectileImpl(GameFunctions.CDecoyProjectile_EmitGrenade(pos, angle, velocity,
            owner?.Address ?? nint.Zero, (uint)HelpersService.WeaponItemDefinitionIndices["weapon_decoy"]));
    }

    public static Task<CDecoyProjectile> EmitGrenadeAsync( Vector pos, QAngle angle, Vector velocity,
        CBasePlayerPawn? owner )
    {
        NativeBinding.ThrowIfNonMainThread();
        return SchedulerManager.QueueOrNow(() => EmitGrenade(pos, angle, velocity, owner));
    }
}