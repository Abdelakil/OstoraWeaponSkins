namespace SwiftlyS2.Shared.Events;

/// <summary>
/// Called when a client sends a voice packet.
/// </summary>
public interface IOnClientVoiceEvent
{
    /// <summary>
    /// The player ID of the client that sent a voice packet.
    /// </summary>
    public int PlayerId { get; }
}
