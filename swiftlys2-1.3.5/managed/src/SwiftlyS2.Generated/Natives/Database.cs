#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeDatabase
{

    private unsafe static delegate* unmanaged<int*, byte*> _GetDefaultDriver;

    public unsafe static string GetDefaultDriver()
    {
        var length = 0;
        var returnedPtr = _GetDefaultDriver(&length);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<int*, byte*> _GetDefaultConnectionName;

    public unsafe static string GetDefaultConnectionName()
    {
        var length = 0;
        var returnedPtr = _GetDefaultConnectionName(&length);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionDriver;

    public unsafe static string GetConnectionDriver(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionDriver(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionHost;

    public unsafe static string GetConnectionHost(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionHost(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionDatabase;

    public unsafe static string GetConnectionDatabase(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionDatabase(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionUser;

    public unsafe static string GetConnectionUser(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionUser(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionPass;

    public unsafe static string GetConnectionPass(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionPass(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<byte*, uint> _GetConnectionTimeout;

    public unsafe static uint GetConnectionTimeout(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var ret = _GetConnectionTimeout((byte*)connectionNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<byte*, ushort> _GetConnectionPort;

    public unsafe static ushort GetConnectionPort(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var ret = _GetConnectionPort((byte*)connectionNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*> _GetConnectionRawUri;

    public unsafe static string GetConnectionRawUri(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetConnectionRawUri(&length, (byte*)connectionNameBufferPtr);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte> _ConnectionExists;

    public unsafe static bool ConnectionExists(string connectionName)
    {
        return StringAlloc.CreateCString(connectionName, connectionNameBufferPtr =>
        {
            var ret = _ConnectionExists((byte*)connectionNameBufferPtr);
            return ret == 1;
        });
    }
}