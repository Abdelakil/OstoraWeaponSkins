#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeServerHelpers
{

    private unsafe static delegate* unmanaged<int*, byte*> _GetServerLanguage;

    public unsafe static string GetServerLanguage()
    {
        var length = 0;
        var returnedPtr = _GetServerLanguage(&length);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<byte> _UsePlayerLanguage;

    public unsafe static bool UsePlayerLanguage()
    {
        var ret = _UsePlayerLanguage();
        return ret == 1;
    }

    private unsafe static delegate* unmanaged<byte> _IsFollowingServerGuidelines;

    public unsafe static bool IsFollowingServerGuidelines()
    {
        var ret = _IsFollowingServerGuidelines();
        return ret == 1;
    }

    private unsafe static delegate* unmanaged<byte> _UseAutoHotReload;

    public unsafe static bool UseAutoHotReload()
    {
        var ret = _UseAutoHotReload();
        return ret == 1;
    }
}