using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.Scheduler;
using SwiftlyS2.Core.Services;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CMolotovProjectileImpl : CMolotovProjectile
{
    public static CMolotovProjectile EmitGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
    {
        NativeBinding.ThrowIfNonMainThread();
        return new CMolotovProjectileImpl(GameFunctions.CMolotovProjectile_EmitGrenade(pos, angle, velocity,
            owner?.Address ?? nint.Zero,
            (uint)HelpersService.WeaponItemDefinitionIndices[
                team == Team.CT ? "weapon_incgrenade" : "weapon_molotov"]));
    }

    public static Task<CMolotovProjectile> EmitGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
    {
        return SchedulerManager.QueueOrNow(() => EmitGrenade(pos, angle, velocity, team, owner));
    }
}