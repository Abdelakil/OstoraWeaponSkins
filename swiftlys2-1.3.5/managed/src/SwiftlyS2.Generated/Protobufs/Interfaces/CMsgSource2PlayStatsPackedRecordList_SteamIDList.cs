using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CMsgSource2PlayStatsPackedRecordList_SteamIDList : ITypedProtobuf<CMsgSource2PlayStatsPackedRecordList_SteamIDList>
{
    static CMsgSource2PlayStatsPackedRecordList_SteamIDList ITypedProtobuf<CMsgSource2PlayStatsPackedRecordList_SteamIDList>.Wrap(nint handle, bool isManuallyAllocated) => new CMsgSource2PlayStatsPackedRecordList_SteamIDListImpl(handle, isManuallyAllocated);

    public IProtobufRepeatedFieldValueType<ulong> Steamid { get; }
}