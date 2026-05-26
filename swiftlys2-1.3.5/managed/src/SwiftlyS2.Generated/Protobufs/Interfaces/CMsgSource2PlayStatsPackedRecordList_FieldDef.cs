using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CMsgSource2PlayStatsPackedRecordList_FieldDef : ITypedProtobuf<CMsgSource2PlayStatsPackedRecordList_FieldDef>
{
    static CMsgSource2PlayStatsPackedRecordList_FieldDef ITypedProtobuf<CMsgSource2PlayStatsPackedRecordList_FieldDef>.Wrap(nint handle, bool isManuallyAllocated) => new CMsgSource2PlayStatsPackedRecordList_FieldDefImpl(handle, isManuallyAllocated);

    public string FieldName { get; set; }
    public ESource2PlayStatsFieldType FieldType { get; set; }
}