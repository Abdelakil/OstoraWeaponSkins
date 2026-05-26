using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CChinaAgreementSessions_StartAgreementSessionInGame_Response : ITypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Response>
{
    static CChinaAgreementSessions_StartAgreementSessionInGame_Response ITypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Response>.Wrap(nint handle, bool isManuallyAllocated) => new CChinaAgreementSessions_StartAgreementSessionInGame_ResponseImpl(handle, isManuallyAllocated);

    public string AgreementUrl { get; set; }
}