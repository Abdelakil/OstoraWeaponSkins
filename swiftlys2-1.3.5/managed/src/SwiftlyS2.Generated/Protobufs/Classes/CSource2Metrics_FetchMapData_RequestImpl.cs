using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CSource2Metrics_FetchMapData_RequestImpl : TypedProtobuf<CSource2Metrics_FetchMapData_Request>, CSource2Metrics_FetchMapData_Request
{
    public CSource2Metrics_FetchMapData_RequestImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public uint Appid
    { get => Accessor.GetUInt32("appid"); set => Accessor.SetUInt32("appid", value); }
    public string MapName
    { get => Accessor.GetString("map_name"); set => Accessor.SetString("map_name", value); }
    public uint GameType
    { get => Accessor.GetUInt32("game_type"); set => Accessor.SetUInt32("game_type", value); }
    public uint GameMode
    { get => Accessor.GetUInt32("game_mode"); set => Accessor.SetUInt32("game_mode", value); }
    public string Param
    { get => Accessor.GetString("param"); set => Accessor.SetString("param", value); }
    public uint TimeSpan
    { get => Accessor.GetUInt32("time_span"); set => Accessor.SetUInt32("time_span", value); }
}