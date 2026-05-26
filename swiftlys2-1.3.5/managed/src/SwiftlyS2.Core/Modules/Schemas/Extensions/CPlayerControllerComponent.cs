using SwiftlyS2.Shared.Players;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CPlayerControllerComponent
{
    /// <summary>
    /// Gets the player controller associated with this instance.
    /// </summary>
    public CBasePlayerController Controller { get; }

    /// <summary>
    /// Attempts to convert this component's pawn to an <see cref="IPlayer"/> instance.
    /// </summary>
    public IPlayer? ToPlayer();
}