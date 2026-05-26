using SwiftlyS2.Core.Natives;
using SwiftlyS2.Core.NetMessages;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SwiftlyS2.Core.ProtobufDefinitions;

internal class CUserMsg_ParticleManager_SetOverrideTextureImpl : TypedProtobuf<CUserMsg_ParticleManager_SetOverrideTexture>, CUserMsg_ParticleManager_SetOverrideTexture
{
    public CUserMsg_ParticleManager_SetOverrideTextureImpl(nint handle, bool isManuallyAllocated) : base(handle)
    {
    }

    public string TextureName
    { get => Accessor.GetString("texture_name"); set => Accessor.SetString("texture_name", value); }
}