using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CMsgSource2PlayStatsPackedRecordList_FieldDefImpl : TypedProtobuf<CMsgSource2PlayStatsPackedRecordList_FieldDef>, CMsgSource2PlayStatsPackedRecordList_FieldDef
{
    public CMsgSource2PlayStatsPackedRecordList_FieldDefImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public string FieldName
    { get => Accessor.GetString("field_name"); set => Accessor.SetString("field_name", value); }
    public ESource2PlayStatsFieldType FieldType
    { get => (ESource2PlayStatsFieldType)Accessor.GetInt32("field_type"); set => Accessor.SetInt32("field_type", (int)value); }
}