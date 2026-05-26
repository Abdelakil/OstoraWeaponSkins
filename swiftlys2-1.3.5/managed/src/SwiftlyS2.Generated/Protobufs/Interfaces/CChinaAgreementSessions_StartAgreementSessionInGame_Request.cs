using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CChinaAgreementSessions_StartAgreementSessionInGame_Request : ITypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Request>
{
    static CChinaAgreementSessions_StartAgreementSessionInGame_Request ITypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Request>.Wrap(nint handle, bool isManuallyAllocated) => new CChinaAgreementSessions_StartAgreementSessionInGame_RequestImpl(handle, isManuallyAllocated);

    public uint Appid { get; set; }
    public ulong Steamid { get; set; }
    public string ClientIpaddress { get; set; }
}