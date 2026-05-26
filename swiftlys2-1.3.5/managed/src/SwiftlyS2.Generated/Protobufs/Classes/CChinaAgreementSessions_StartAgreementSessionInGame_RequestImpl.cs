using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CChinaAgreementSessions_StartAgreementSessionInGame_RequestImpl : TypedProtobuf<CChinaAgreementSessions_StartAgreementSessionInGame_Request>, CChinaAgreementSessions_StartAgreementSessionInGame_Request
{
    public CChinaAgreementSessions_StartAgreementSessionInGame_RequestImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public uint Appid
    { get => Accessor.GetUInt32("appid"); set => Accessor.SetUInt32("appid", value); }
    public ulong Steamid
    { get => Accessor.GetUInt64("steamid"); set => Accessor.SetUInt64("steamid", value); }
    public string ClientIpaddress
    { get => Accessor.GetString("client_ipaddress"); set => Accessor.SetString("client_ipaddress", value); }
}