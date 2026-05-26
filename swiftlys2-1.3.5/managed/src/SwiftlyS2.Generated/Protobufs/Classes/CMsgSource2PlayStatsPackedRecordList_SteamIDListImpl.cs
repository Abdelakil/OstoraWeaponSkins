using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CMsgSource2PlayStatsPackedRecordList_SteamIDListImpl : TypedProtobuf<CMsgSource2PlayStatsPackedRecordList_SteamIDList>, CMsgSource2PlayStatsPackedRecordList_SteamIDList
{
    public CMsgSource2PlayStatsPackedRecordList_SteamIDListImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public IProtobufRepeatedFieldValueType<ulong> Steamid
    { get => new ProtobufRepeatedFieldValueType<ulong>(Accessor, "steamid"); }
}