using System.Runtime.InteropServices;

namespace SwiftlyS2.Core.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
internal struct NativeFunction
{
    public nint Name;
    public nint Function;
}