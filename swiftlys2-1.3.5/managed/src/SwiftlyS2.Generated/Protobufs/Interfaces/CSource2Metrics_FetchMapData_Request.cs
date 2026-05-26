using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CSource2Metrics_FetchMapData_Request : ITypedProtobuf<CSource2Metrics_FetchMapData_Request>
{
    static CSource2Metrics_FetchMapData_Request ITypedProtobuf<CSource2Metrics_FetchMapData_Request>.Wrap(nint handle, bool isManuallyAllocated) => new CSource2Metrics_FetchMapData_RequestImpl(handle, isManuallyAllocated);

    public uint Appid { get; set; }
    public string MapName { get; set; }
    public uint GameType { get; set; }
    public uint GameMode { get; set; }
    public string Param { get; set; }
    public uint TimeSpan { get; set; }
}