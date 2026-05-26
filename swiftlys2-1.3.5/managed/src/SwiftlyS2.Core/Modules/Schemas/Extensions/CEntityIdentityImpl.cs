using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Core.Extensions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Core.EntitySystem;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CEntityIdentityImpl
{

  public CEntityInstance EntityInstance => EntityManager.GetEntityByAddress(Address.Read<nint>()) ?? new CEntityInstanceImpl(Address.Read<nint>());

  public CHandle<CEntityInstance> EntityHandle => new(Address.Read<uint>(0x10));

}