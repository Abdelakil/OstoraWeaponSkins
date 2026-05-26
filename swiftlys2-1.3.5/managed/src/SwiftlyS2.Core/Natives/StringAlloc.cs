using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;

namespace SwiftlyS2.Core.Natives;

internal static class StringAlloc
{
    private static readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
    public unsafe delegate void CStringAction( byte* cstr );

    public static void CreateCString( string str, Action<nint> action )
    {
        var totalByteCount = Encoding.UTF8.GetByteCount(str) + 1;
        var stringBuffer = arrayPool.Rent(totalByteCount);

        _ = Encoding.UTF8.GetBytes(str.AsSpan(), stringBuffer.AsSpan());
        stringBuffer[totalByteCount - 1] = 0;

        unsafe
        {
            fixed (byte* cstr = stringBuffer)
            {
                action((nint)cstr);
            }
        }

        arrayPool.Return(stringBuffer);
    }

    public static T CreateCString<T>( string str, Func<nint, T> action )
    {
        var totalByteCount = Encoding.UTF8.GetByteCount(str) + 1;
        var stringBuffer = arrayPool.Rent(totalByteCount);

        _ = Encoding.UTF8.GetBytes(str.AsSpan(), stringBuffer.AsSpan());
        stringBuffer[totalByteCount - 1] = 0;

        unsafe
        {
            fixed (byte* cstr = stringBuffer)
            {
                var result = action((nint)cstr);
                arrayPool.Return(stringBuffer);
                return result;
            }
        }
    }

    public static string CreateCSharpString( int length, Action<nint> action )
    {
        var totalByteCount = length + 1;
        var stringBuffer = arrayPool.Rent(totalByteCount);

        unsafe
        {
            fixed (byte* cstr = stringBuffer)
            {
                action((nint)cstr);
                var returnString = Encoding.UTF8.GetString(cstr, totalByteCount - 1);
                arrayPool.Return(stringBuffer);
                return returnString;
            }
        }
    }

    public static string CreateCSharpString( nint cstrPtr, int length )
    {
        if (cstrPtr == 0 || length == 0) return "";

        return Marshal.PtrToStringUTF8(cstrPtr, length);
    }

    public static string CreateCSharpString( nint cstrPtr )
    {
        if (cstrPtr == 0) return "";
        return Marshal.PtrToStringUTF8(cstrPtr) ?? "(null)";
    }
}