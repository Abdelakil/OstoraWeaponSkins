using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Shared.Services;

public interface IGameService
{
    /// <summary>
    /// Gets a read-only reference to the current match data.
    /// </summary>
    public ref readonly CCSMatch MatchData { get; }

    /// <summary>
    /// Resets all match data to initial state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Sets the current game phase.
    /// </summary>
    /// <param name="phase">The game phase to set.</param>
    public void SetPhase( GamePhase phase );

    /// <summary>
    /// Adds wins to the Terrorist team.
    /// </summary>
    /// <param name="numWins">Number of wins to add.</param>
    public void AddTerroristWins( int numWins );

    /// <summary>
    /// Adds wins to the Counter-Terrorist team.
    /// </summary>
    /// <param name="numWins">Number of wins to add.</param>
    public void AddCTWins( int numWins );

    /// <summary>
    /// Increments the round count.
    /// </summary>
    /// <param name="numRounds">Number of rounds to increment.</param>
    public void IncrementRound( int numRounds = 1 );

    /// <summary>
    /// Adds bonus points to the Terrorist team.
    /// </summary>
    /// <param name="points">Bonus points to add.</param>
    public void AddTerroristBonusPoints( int points );

    /// <summary>
    /// Adds bonus points to the Counter-Terrorist team.
    /// </summary>
    /// <param name="points">Bonus points to add.</param>
    public void AddCTBonusPoints( int points );

    /// <summary>
    /// Adds score to the Terrorist team.
    /// </summary>
    /// <param name="score">Score to add.</param>
    public void AddTerroristScore( int score );

    /// <summary>
    /// Adds score to the Counter-Terrorist team.
    /// </summary>
    /// <param name="score">Score to add.</param>
    public void AddCTScore( int score );

    /// <summary>
    /// Enters overtime mode.
    /// </summary>
    /// <param name="numOvertimesToAdd">Number of overtime periods to add.</param>
    public void GoToOvertime( int numOvertimesToAdd = 1 );

    /// <summary>
    /// Swaps the team scores between Terrorist and Counter-Terrorist.
    /// </summary>
    public void SwapTeamScores();

    // /// <summary>
    // /// Updates the team score entities based on current match data.
    // /// </summary>
    // void UpdateTeamScores();

    /// <summary>
    /// Gets the winning team ID.
    /// </summary>
    /// <returns>Team ID of the winner, or 0 if tie.</returns>
    public int GetWinningTeam();

    /// <summary>
    /// Ends the current round with the specified reason after an optional delay
    /// </summary>
    /// <param name="reason">The reason for ending the round</param>
    /// <param name="delay">The delay before ending the round</param>
    public void TerminateRound( RoundEndReason reason, float delay );

    
    /// <summary>
    /// Go to the intermission phase of the game.
    /// </summary>
    /// <param name="abortedMatch">Indicates whether the match was aborted</param>
    public void GoToIntermission(bool abortedMatch = false);

    /// <summary>
    /// Creates a HE grenade projectile.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="pos">The position where the HE grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the HE grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the HE grenade projectile.</param>
    /// <param name="owner">The owner of the HE grenade projectile.</param>
    /// <returns>The created HE grenade projectile.</returns>
    [ThreadUnsafe]
    public CHEGrenadeProjectile EmitHEGrenade( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a HE grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the HE grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the HE grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the HE grenade projectile.</param>
    /// <param name="owner">The owner of the HE grenade projectile.</param>
    /// <returns>The created HE grenade projectile.</returns>
    public Task<CHEGrenadeProjectile> EmitHEGrenadeAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a flashbang grenade projectile.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="pos">The position where the flashbang grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the flashbang grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the flashbang grenade projectile.</param>
    /// <param name="owner">The owner of the flashbang grenade projectile.</param>
    /// <returns>The created flashbang grenade projectile.</returns>
    [ThreadUnsafe]
    public CFlashbangProjectile EmitFlashbang( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a flashbang grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the flashbang grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the flashbang grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the flashbang grenade projectile.</param>
    /// <param name="owner">The owner of the flashbang grenade projectile.</param>
    /// <returns>The created flashbang grenade projectile.</returns>
    public Task<CFlashbangProjectile> EmitFlashbangAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );

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
    public CSmokeGrenadeProjectile EmitSmokeGrenade( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner );

    /// <summary>   
    /// Creates a smoke grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the smoke grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the smoke grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the smoke grenade projectile.</param>
    /// <param name="team">The team associated with the smoke grenade projectile.</param>
    /// <param name="owner">The owner of the smoke grenade projectile.</param>
    /// <returns>The created smoke grenade projectile.</returns>
    public Task<CSmokeGrenadeProjectile> EmitSmokeGrenadeAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner );

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
    public CMolotovProjectile EmitMolotov( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a molotov grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the molotov grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the molotov grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the molotov grenade projectile.</param>
    /// <param name="team">The team of the molotov grenade projectile.</param>
    /// <param name="owner">The owner of the molotov grenade projectile.</param>
    /// <returns>The created molotov grenade projectile.</returns>
    public Task<CMolotovProjectile> EmitMolotovAsync( Vector pos, QAngle angle, Vector velocity, Team team,
        CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a decoy grenade projectile.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="pos">The position where the decoy grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the decoy grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the decoy grenade projectile.</param>
    /// <param name="owner">The owner of the decoy grenade projectile.</param>
    /// <returns>The created decoy grenade projectile.</returns>
    [ThreadUnsafe]
    public CDecoyProjectile EmitDecoy( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );

    /// <summary>
    /// Creates a decoy grenade projectile asynchronously.
    /// </summary>
    /// <param name="pos">The position where the decoy grenade projectile will be created.</param>
    /// <param name="angle">The angle at which the decoy grenade projectile will be created.</param>
    /// <param name="velocity">The velocity of the decoy grenade projectile.</param>
    /// <param name="owner">The owner of the decoy grenade projectile.</param>
    /// <returns>The created decoy grenade projectile.</returns>
    public Task<CDecoyProjectile> EmitDecoyAsync( Vector pos, QAngle angle, Vector velocity, CBasePlayerPawn? owner );
}