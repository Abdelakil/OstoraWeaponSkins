using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CCSObserverPawnImpl : CCSObserverPawn
{
    public new CCSObserver_ObserverServices? ObserverServices => base.ObserverServices?.As<CCSObserver_ObserverServices>();
    public new CCSObserver_MovementServices? MovementServices => base.MovementServices?.As<CCSObserver_MovementServices>();
    public new CCSObserver_CameraServices? CameraServices => base.CameraServices?.As<CCSObserver_CameraServices>();
    public new CCSObserver_UseServices? UseServices => base.UseServices?.As<CCSObserver_UseServices>();
}