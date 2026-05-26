using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CSource2Metrics_FetchMapData_Response_MapDataImpl : TypedProtobuf<CSource2Metrics_FetchMapData_Response_MapData>, CSource2Metrics_FetchMapData_Response_MapData
{
    public CSource2Metrics_FetchMapData_Response_MapDataImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public string Name
    { get => Accessor.GetString("name"); set => Accessor.SetString("name", value); }
    public string Type
    { get => Accessor.GetString("type"); set => Accessor.SetString("type", value); }
    public string Data
    { get => Accessor.GetString("data"); set => Accessor.SetString("data", value); }
}