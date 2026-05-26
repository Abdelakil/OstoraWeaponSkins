using SwiftlyS2.Core.Natives.NativeObjects;
using SwiftlyS2.Shared.Schemas;

namespace SwiftlyS2.Core.Schemas;

internal abstract class SchemaField : NativeHandle, ISchemaField
{
    public SchemaField( nint handle, ulong hash ) : base(handle + Schema.GetOffset(hash))
    {
    }
}
