using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Core.SchemaDefinitions;
using SwiftlyS2.Shared.Misc;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CMolotovProjectile
{
    /// <summary>
    /// Creates a molotov grenade projectile.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="pos">The position where the molotov grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the molotov grenade projectile will be created.</param>
    /// <param name="team">The team of the molotov grenade projectile.</param>
    /// <param name="velocity">The velocity of the molotov grenade projectile.</param>
    /// <param name="owner">The owner of the molotov grenade projectile.</param>
    /// <returns>The created molotov grenade projectile.</returns>
    [ThreadUnsafe]
    public static CMolotovProjectile EmitGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CMolotovProjectileImpl.EmitGrenade(pos, angle, velocity, team, owner);

    /// <summary>
    /// Creates a molotov grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the molotov grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the molotov grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the molotov grenade projectile.</param>
    /// <param name="team">The team of the molotov grenade projectile.</param>
    /// <param name="owner">The owner of the molotov grenade projectile.</param>
    /// <returns>The created molotov grenade projectile.</returns>
    public Task<CMolotovProjectile> EmitGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner )
        => CMolotovProjectileImpl.EmitGrenadeAsync(pos, angle, velocity, team, owner);
}