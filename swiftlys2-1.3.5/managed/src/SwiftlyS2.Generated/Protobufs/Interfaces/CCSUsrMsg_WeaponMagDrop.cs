using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CCSUsrMsg_WeaponMagDrop : ITypedProtobuf<CCSUsrMsg_WeaponMagDrop>, INetMessage<CCSUsrMsg_WeaponMagDrop>, IDisposable
{
    static int INetMessage<CCSUsrMsg_WeaponMagDrop>.MessageId => 389;

    static string INetMessage<CCSUsrMsg_WeaponMagDrop>.MessageName => "CCSUsrMsg_WeaponMagDrop";

    static CCSUsrMsg_WeaponMagDrop ITypedProtobuf<CCSUsrMsg_WeaponMagDrop>.Wrap(nint handle, bool isManuallyAllocated) => new CCSUsrMsg_WeaponMagDropImpl(handle, isManuallyAllocated);

    public int Entidx { get; set; }
    public int SecondaryData { get; set; }
    public bool ServerEvent { get; set; }
}