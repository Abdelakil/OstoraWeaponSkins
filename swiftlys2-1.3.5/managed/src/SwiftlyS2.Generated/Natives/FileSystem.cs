#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeFileSystem
{

    private unsafe static delegate* unmanaged<int*, byte*, int, int, byte*> _GetSearchPath;

    public unsafe static string GetSearchPath(string pathId, int searchPathType, int searchPathsToGet)
    {
        return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
        {
            var length = 0;
            var returnedPtr = _GetSearchPath(&length, (byte*)pathIdBufferPtr, searchPathType, searchPathsToGet);
            var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
            NativeAllocator.Free((nint)returnedPtr);
            return outString;
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, int, int, void> _AddSearchPath;

    public unsafe static void AddSearchPath(string path, string pathId, int searchPathAdd, int searchPathPriority)
    {
        if (!NativeBinding.IsMainThread)
        {
            throw new InvalidOperationException("This method can only be called from the main thread.");
        }
        StringAlloc.CreateCString(path, pathBufferPtr =>
        {
            StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                _AddSearchPath((byte*)pathBufferPtr, (byte*)pathIdBufferPtr, searchPathAdd, searchPathPriority);
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte> _RemoveSearchPath;

    public unsafe static bool RemoveSearchPath(string path, string pathId)
    {
        return StringAlloc.CreateCString(path, pathBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _RemoveSearchPath((byte*)pathBufferPtr, (byte*)pathIdBufferPtr);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte> _FileExists;

    public unsafe static bool FileExists(string fileName, string pathId)
    {
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _FileExists((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte> _IsDirectory;

    public unsafe static bool IsDirectory(string path, string pathId)
    {
        return StringAlloc.CreateCString(path, pathBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _IsDirectory((byte*)pathBufferPtr, (byte*)pathIdBufferPtr);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<void> _PrintSearchPaths;

    public unsafe static void PrintSearchPaths()
    {
        _PrintSearchPaths();
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*, byte*> _ReadFile;

    public unsafe static string ReadFile(string fileName, string pathId)
    {
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var length = 0;
                var returnedPtr = _ReadFile(&length, (byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr);
                var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
                NativeAllocator.Free((nint)returnedPtr);
                return outString;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte*, byte> _WriteFile;

    public unsafe static bool WriteFile(string fileName, string pathId, string content)
    {
        if (!NativeBinding.IsMainThread)
        {
            throw new InvalidOperationException("This method can only be called from the main thread.");
        }
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                return StringAlloc.CreateCString(content, contentBufferPtr =>
                {
                    var ret = _WriteFile((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr, (byte*)contentBufferPtr);
                    return ret == 1;
                });
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, uint> _GetFileSize;

    public unsafe static uint GetFileSize(string fileName, string pathId)
    {
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _GetFileSize((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr);
                return ret;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte> _PrecacheFile;

    public unsafe static bool PrecacheFile(string fileName, string pathId)
    {
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _PrecacheFile((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte> _IsFileWritable;

    public unsafe static bool IsFileWritable(string fileName, string pathId)
    {
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _IsFileWritable((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte, byte> _SetFileWritable;

    public unsafe static bool SetFileWritable(string fileName, string pathId, bool writable)
    {
        if (!NativeBinding.IsMainThread)
        {
            throw new InvalidOperationException("This method can only be called from the main thread.");
        }
        return StringAlloc.CreateCString(fileName, fileNameBufferPtr =>
        {
            return StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                var ret = _SetFileWritable((byte*)fileNameBufferPtr, (byte*)pathIdBufferPtr, writable ? (byte)1 : (byte)0);
                return ret == 1;
            });
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, byte*, void> _FindFileAbsoluteList;

    public unsafe static void FindFileAbsoluteList(nint outVector, string wildcard, string pathId)
    {
        StringAlloc.CreateCString(wildcard, wildcardBufferPtr =>
        {
            StringAlloc.CreateCString(pathId, pathIdBufferPtr =>
            {
                _FindFileAbsoluteList(outVector, (byte*)wildcardBufferPtr, (byte*)pathIdBufferPtr);
            });
        });
    }
}