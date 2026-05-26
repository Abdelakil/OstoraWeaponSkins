using SwiftlyS2.Shared.Players;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CPlayerPawnComponent
{
    /// <summary>
    /// Gets the player pawn associated with this instance.
    /// </summary>
    public CBasePlayerPawn Pawn { get; }

    /// <summary>
    /// Attempts to convert this component's pawn to an <see cref="IPlayer"/> instance.
    /// </summary>
    public IPlayer? ToPlayer();
}