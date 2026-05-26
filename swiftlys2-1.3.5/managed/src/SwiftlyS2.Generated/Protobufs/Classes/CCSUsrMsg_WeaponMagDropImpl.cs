using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CCSUsrMsg_WeaponMagDropImpl : NetMessage<CCSUsrMsg_WeaponMagDrop>, CCSUsrMsg_WeaponMagDrop
{
    public CCSUsrMsg_WeaponMagDropImpl(nint handle, bool isManuallyAllocated) : base(handle, isManuallyAllocated)
    {
    }

    public int Entidx
    { get => Accessor.GetInt32("entidx"); set => Accessor.SetInt32("entidx", value); }
    public int SecondaryData
    { get => Accessor.GetInt32("secondary_data"); set => Accessor.SetInt32("secondary_data", value); }
    public bool ServerEvent
    { get => Accessor.GetBool("server_event"); set => Accessor.SetBool("server_event", value); }
}