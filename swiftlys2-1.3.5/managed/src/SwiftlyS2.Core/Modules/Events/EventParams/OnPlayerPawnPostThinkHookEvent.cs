using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.Events;

internal class OnPlayerPawnPostThinkHookEvent : IOnPlayerPawnPostThinkHookEvent, IDisposable
{
    public required CCSPlayerPawn PlayerPawn { get; init; }

    public void Dispose()
    {
    }
}