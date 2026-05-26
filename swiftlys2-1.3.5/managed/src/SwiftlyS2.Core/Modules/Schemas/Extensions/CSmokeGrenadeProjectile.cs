using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Core.SchemaDefinitions;
using SwiftlyS2.Shared.Misc;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CSmokeGrenadeProjectile
{
    /// <summary>
    /// Creates a smoke grenade projectile.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="pos">The position where the smoke grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the smoke grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the smoke grenade projectile.</param>
    /// <param name="team">The team associated with the smoke grenade projectile.</param>
    /// <param name="owner">The owner of the smoke grenade projectile.</param>
    /// <returns>The created smoke grenade projectile.</returns>
    [ThreadUnsafe]
    public static CSmokeGrenadeProjectile EmitGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CSmokeGrenadeProjectileImpl.EmitGrenade(pos, angle, velocity, team, owner);

    /// <summary>   
    /// Creates a smoke grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the smoke grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the smoke grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the smoke grenade projectile.</param>
    /// <param name="team">The team associated with the smoke grenade projectile.</param>
    /// <param name="owner">The owner of the smoke grenade projectile.</param>
    /// <returns>The created smoke grenade projectile.</returns>
    public Task<CSmokeGrenadeProjectile> EmitGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CSmokeGrenadeProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, team, owner);
}