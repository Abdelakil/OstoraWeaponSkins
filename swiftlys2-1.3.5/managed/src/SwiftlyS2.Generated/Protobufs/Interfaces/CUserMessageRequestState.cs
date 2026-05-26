using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CUserMessageRequestState : ITypedProtobuf<CUserMessageRequestState>, INetMessage<CUserMessageRequestState>, IDisposable
{
    static int INetMessage<CUserMessageRequestState>.MessageId => 114;

    static string INetMessage<CUserMessageRequestState>.MessageName => "CUserMessageRequestState";

    static CUserMessageRequestState ITypedProtobuf<CUserMessageRequestState>.Wrap(nint handle, bool isManuallyAllocated) => new CUserMessageRequestStateImpl(handle, isManuallyAllocated);

}