using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CSource2Metrics_RecordPlayStats_NotificationImpl : TypedProtobuf<CSource2Metrics_RecordPlayStats_Notification>, CSource2Metrics_RecordPlayStats_Notification
{
    public CSource2Metrics_RecordPlayStats_NotificationImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public IProtobufRepeatedFieldSubMessageType<CMsgSource2PlayStatsPackedRecordList> RecordTypes
    { get => new ProtobufRepeatedFieldSubMessageType<CMsgSource2PlayStatsPackedRecordList>(Accessor, "record_types"); }
    public uint Appid
    { get => Accessor.GetUInt32("appid"); set => Accessor.SetUInt32("appid", value); }
}