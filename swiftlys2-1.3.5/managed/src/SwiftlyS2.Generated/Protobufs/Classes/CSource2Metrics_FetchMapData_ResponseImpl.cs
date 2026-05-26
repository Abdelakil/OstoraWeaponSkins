using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CSource2Metrics_FetchMapData_ResponseImpl : TypedProtobuf<CSource2Metrics_FetchMapData_Response>, CSource2Metrics_FetchMapData_Response
{
    public CSource2Metrics_FetchMapData_ResponseImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public IProtobufRepeatedFieldSubMessageType<CSource2Metrics_FetchMapData_Response_MapData> Results
    { get => new ProtobufRepeatedFieldSubMessageType<CSource2Metrics_FetchMapData_Response_MapData>(Accessor, "results"); }
}