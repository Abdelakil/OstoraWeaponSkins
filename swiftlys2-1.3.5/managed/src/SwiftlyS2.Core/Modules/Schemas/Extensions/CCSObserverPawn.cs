namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CCSObserverPawn
{
    public new CCSObserver_ObserverServices? ObserverServices { get; }
    public new CCSObserver_MovementServices? MovementServices { get; }
    public new CCSObserver_CameraServices? CameraServices { get; }
    public new CCSObserver_UseServices? UseServices { get; }
}