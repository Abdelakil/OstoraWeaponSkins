using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.Scheduler;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CCSPlayerControllerImpl
{
    public void Respawn()
    {
        NativeBinding.ThrowIfNonMainThread();
        var pawn = PlayerPawn;
        if (pawn is { IsValid: false }) return;

        SetPawn(pawn.Value!);
        GameFunctions.CCSPlayerController_Respawn(Address);
    }

    public Task RespawnAsync()
    {
        return SchedulerManager.QueueOrNow(Respawn);
    }
}