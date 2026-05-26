using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CChinaAgreementSessions_StartAgreementSessionInGame_ResponseImpl : TypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Response>, CChinaAgreementSessions_StartAgreementSessionInGame_Response
{
    public CChinaAgreementSessions_StartAgreementSessionInGame_ResponseImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public string AgreementUrl
    { get => Accessor.GetString("agreement_url"); set => Accessor.SetString("agreement_url", value); }
}