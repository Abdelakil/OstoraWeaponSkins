using SwiftlyS2.Core.ProtobufDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.NetMessages;

namespace SwiftlyS2.Shared.ProtobufDefinitions;

public interface CUserMsg_ParticleManager_CreateSmokeGrid : ITypedProtobuf<CUserMsg_ParticleManager_CreateSmokeGrid>
{
    static CUserMsg_ParticleManager_CreateSmokeGrid ITypedProtobuf<CUserMsg_ParticleManager_CreateSmokeGrid>.Wrap(nint handle, bool isManuallyAllocated) => new CUserMsg_ParticleManager_CreateSmokeGridImpl(handle, isManuallyAllocated);

    public string VdataName { get; set; }
}