using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CSource2Metrics_RecordPlayStats_Notification : ITypedProtobuf<CSource2Metrics_RecordPlayStats_Notification>
{
    static CSource2Metrics_RecordPlayStats_Notification ITypedProtobuf<CSource2Metrics_RecordPlayStats_Notification>.Wrap(nint handle, bool isManuallyAllocated) => new CSource2Metrics_RecordPlayStats_NotificationImpl(handle, isManuallyAllocated);

    public IProtobufRepeatedFieldSubMessageType<CMsgSource2PlayStatsPackedRecordList> RecordTypes { get; }
    public uint Appid { get; set; }
}