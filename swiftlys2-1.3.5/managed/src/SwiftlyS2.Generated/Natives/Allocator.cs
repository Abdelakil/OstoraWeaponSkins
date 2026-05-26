#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeAllocator
{

    private unsafe static delegate* unmanaged<ulong, nint> _Alloc;

    public unsafe static nint Alloc(ulong size)
    {
        var ret = _Alloc(size);
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, void> _Free;

    public unsafe static void Free(nint pointer)
    {
        _Free(pointer);
    }

    private unsafe static delegate* unmanaged<nint, ulong, nint> _Resize;

    public unsafe static nint Resize(nint pointer, ulong new_size)
    {
        var ret = _Resize(pointer, new_size);
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, ulong> _GetSize;

    /// <summary>
    /// works only for pointers allocated through Memory.Allocator
    /// </summary>
    public unsafe static ulong GetSize(nint pointer)
    {
        var ret = _GetSize(pointer);
        return ret;
    }

    private unsafe static delegate* unmanaged<ulong> _GetTotalAllocated;

    public unsafe static ulong GetTotalAllocated()
    {
        var ret = _GetTotalAllocated();
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, byte> _IsPointerValid;

    public unsafe static bool IsPointerValid(nint pointer)
    {
        var ret = _IsPointerValid(pointer);
        return ret == 1;
    }

    private unsafe static delegate* unmanaged<nint, nint, ulong, void> _Copy;

    public unsafe static void Copy(nint dst, nint src, ulong size)
    {
        _Copy(dst, src, size);
    }

    private unsafe static delegate* unmanaged<nint, nint, ulong, void> _Move;

    public unsafe static void Move(nint dst, nint src, ulong size)
    {
        _Move(dst, src, size);
    }
}