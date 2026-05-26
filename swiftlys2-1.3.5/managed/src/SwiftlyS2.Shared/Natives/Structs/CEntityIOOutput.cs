using System.Runtime.InteropServices;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Shared.Natives;

[StructLayout(LayoutKind.Sequential)]
public struct EntityIOConnectionDesc_t
{
    public nint TargetDesc;
    public nint TargetInput;
    public nint ValueOverride;
    public uint Target;
    public EntityIOTargetType_t TargetType;
    public int TimesToFire;
    public float Delay;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EntityIOConnection_t
{
    public EntityIOConnectionDesc_t Desc;
    public bool MarkedForRemoval;
    public EntityIOConnection_t* Next;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EntityIOOutputDesc_t
{
    public CString Name;
    public uint Flags;
    public uint OutputOffset;
}

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct CEntityIOOutput
{
    private readonly void* vtable;
    private readonly EntityIOConnection_t* connections;
    private readonly EntityIOOutputDesc_t* desc;

    public readonly ref EntityIOConnection_t Connections => ref *connections;
    public readonly ref EntityIOOutputDesc_t Desc => ref *desc;
}
