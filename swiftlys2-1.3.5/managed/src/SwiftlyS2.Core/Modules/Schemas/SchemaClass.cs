using SwiftlyS2.Core.Natives.NativeObjects;

namespace SwiftlyS2.Core.Schemas;

internal abstract class SchemaClass : NativeHandle
{
  public SchemaClass( nint handle ) : base(handle)
  {
  }

}