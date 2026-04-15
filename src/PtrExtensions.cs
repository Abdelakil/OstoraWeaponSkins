using System.Runtime.CompilerServices;

namespace OstoraWeaponSkins;

internal static class PtrExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this nint ptr) where T : unmanaged
    {
        unsafe { return Unsafe.Read<T>((void*)ptr); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this nint ptr, int offset) where T : unmanaged
    {
        unsafe { return Unsafe.Read<T>((void*)(ptr + offset)); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(this nint ptr) where T : unmanaged, allows ref struct
    {
        unsafe { return ref Unsafe.AsRef<T>((void*)ptr); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(this nint ptr, int offset) where T : unmanaged
    {
        unsafe { return ref Unsafe.AsRef<T>((void*)(ptr + offset)); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AsRef<T>(this nint ptr, nint offset) where T : unmanaged
    {
        unsafe { return ref Unsafe.AsRef<T>((void*)(ptr + offset)); }
    }
}
