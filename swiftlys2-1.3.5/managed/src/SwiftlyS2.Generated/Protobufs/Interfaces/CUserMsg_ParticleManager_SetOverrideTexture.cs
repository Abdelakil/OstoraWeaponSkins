using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CUserMsg_ParticleManager_SetOverrideTexture : ITypedProtobuf<CUserMsg_ParticleManager_SetOverrideTexture>
{
    static CUserMsg_ParticleManager_SetOverrideTexture ITypedProtobuf<CUserMsg_ParticleManager_SetOverrideTexture>.Wrap(nint handle, bool isManuallyAllocated) => new CUserMsg_ParticleManager_SetOverrideTextureImpl(handle, isManuallyAllocated);

    public string TextureName { get; set; }
}