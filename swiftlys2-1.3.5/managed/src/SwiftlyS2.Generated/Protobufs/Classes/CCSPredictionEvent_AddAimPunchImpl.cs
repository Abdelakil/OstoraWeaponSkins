using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CCSPredictionEvent_AddAimPunchImpl : TypedProtobuf<CCSPredictionEvent_AddAimPunch>, CCSPredictionEvent_AddAimPunch
{
    public CCSPredictionEvent_AddAimPunchImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public QAngle PunchAngle
    { get => Accessor.GetQAngle("punch_angle"); set => Accessor.SetQAngle("punch_angle", value); }
}