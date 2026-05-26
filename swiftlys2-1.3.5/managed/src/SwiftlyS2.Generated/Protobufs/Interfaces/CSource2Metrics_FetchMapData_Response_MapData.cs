using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CSource2Metrics_FetchMapData_Response_MapData : ITypedProtobuf<CSource2Metrics_FetchMapData_Response_MapData>
{
    static CSource2Metrics_FetchMapData_Response_MapData ITypedProtobuf<CSource2Metrics_FetchMapData_Response_MapData>.Wrap(nint handle, bool isManuallyAllocated) => new CSource2Metrics_FetchMapData_Response_MapDataImpl(handle, isManuallyAllocated);

    public string Name { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
}