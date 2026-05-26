using SwiftlyS2.Shared.Misc;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CCSPlayerController
{
    /// <summary>
    /// Respawns the player.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    [ThreadUnsafe]
    public void Respawn();

    /// <summary>
    /// Respawns the player asynchronously.
    /// </summary>
    public Task RespawnAsync();
}