#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativePatches
{

    private unsafe static delegate* unmanaged<byte*, void> _Apply;

    public unsafe static void Apply(string patchName)
    {
        StringAlloc.CreateCString(patchName, patchNameBufferPtr =>
        {
            _Apply((byte*)patchNameBufferPtr);
        });
    }

    private unsafe static delegate* unmanaged<byte*, void> _Revert;

    public unsafe static void Revert(string patchName)
    {
        StringAlloc.CreateCString(patchName, patchNameBufferPtr =>
        {
            _Revert((byte*)patchNameBufferPtr);
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte> _Exists;

    public unsafe static bool Exists(string patchName)
    {
        return StringAlloc.CreateCString(patchName, patchNameBufferPtr =>
        {
            var ret = _Exists((byte*)patchNameBufferPtr);
            return ret == 1;
        });
    }
}