using System.Runtime.InteropServices;

namespace SwiftlyS2.Shared.Memory;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Xmm
{
    [FieldOffset(0)]
    public unsafe fixed byte U8[16];

    [FieldOffset(0)]
    public unsafe fixed ushort U16[8];

    [FieldOffset(0)]
    public unsafe fixed uint U32[4];

    [FieldOffset(0)]
    public unsafe fixed ulong U64[2];

    [FieldOffset(0)]
    public unsafe fixed float F32[4];

    [FieldOffset(0)]
    public unsafe fixed double F64[2];
}

[StructLayout(LayoutKind.Sequential)]
public struct MidHookContext
{
    public Xmm XMM0, XMM1, XMM2, XMM3, XMM4, XMM5, XMM6, XMM7, XMM8, XMM9, XMM10, XMM11, XMM12, XMM13, XMM14, XMM15;
    public nuint RFLAGS, R15, R14, R13, R12, R11, R10, R9, R8, RDI, RSI, RDX, RCX, RBX, RAX, RBP, RSP, TRAMPOLINE_RSP, RIP;

    public override readonly unsafe string ToString()
    {
        var xmmRegs = new Xmm[] { XMM0, XMM1, XMM2, XMM3, XMM4, XMM5, XMM6, XMM7, XMM8, XMM9, XMM10, XMM11, XMM12, XMM13, XMM14, XMM15 };
        return string.Join("\n", new[]
        {
            $"    RAX: 0x{RAX:X16}  RBX: 0x{RBX:X16}  RCX: 0x{RCX:X16}  RDX: 0x{RDX:X16}",
            $"    RSI: 0x{RSI:X16}  RDI: 0x{RDI:X16}  RBP: 0x{RBP:X16}  RSP: 0x{RSP:X16}",
            $"    R8:  0x{R8:X16}  R9:  0x{R9:X16}  R10: 0x{R10:X16}  R11: 0x{R11:X16}",
            $"    R12: 0x{R12:X16}  R13: 0x{R13:X16}  R14: 0x{R14:X16}  R15: 0x{R15:X16}",
            string.Join("\n", Enumerable.Range(0, 8).Select(i =>$"    XMM{i:D2}: {xmmRegs[i].U32[0]:X8} {xmmRegs[i].U32[1]:X8} {xmmRegs[i].U32[2]:X8} {xmmRegs[i].U32[3]:X8}  XMM{i+8:D2}: {xmmRegs[i+8].U32[0]:X8} {xmmRegs[i+8].U32[1]:X8} {xmmRegs[i+8].U32[2]:X8} {xmmRegs[i+8].U32[3]:X8}")),
            $"    RIP: 0x{RIP:X16}  RFLAGS: 0x{RFLAGS:X16}",
            $"    TRAMPOLINE_RSP: 0x{TRAMPOLINE_RSP:X16}"
        });
    }
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void MidHookDelegate( ref MidHookContext context );

public interface IUnmanagedMemory
{
    /// <summary>
    /// The address of the unmanaged pointer.
    /// </summary>
    public nint Address { get; }

    /// <summary>
    /// Hook a native function at the specified address with a managed callback.
    /// The callback receives a context structure that allows reading and modifying CPU registers.
    /// </summary>
    /// <param name="callback">The callback to call when the code reaches that address.</param>
    public Guid AddHook( MidHookDelegate callback );

    /// <summary>
    /// Unhook a hook by its id.
    /// </summary>
    /// <param name="id">The id of the hook to unhook.</param>
    public void RemoveHook( Guid id );
}