using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CSource2Metrics_FetchMapData_Response : ITypedProtobuf<CSource2Metrics_FetchMapData_Response>
{
    static CSource2Metrics_FetchMapData_Response ITypedProtobuf<CSource2Metrics_FetchMapData_Response>.Wrap(nint handle, bool isManuallyAllocated) => new CSource2Metrics_FetchMapData_ResponseImpl(handle, isManuallyAllocated);

    public IProtobufRepeatedFieldSubMessageType<CSource2Metrics_FetchMapData_Response_MapData> Results { get; }
}