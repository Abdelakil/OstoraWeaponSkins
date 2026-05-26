#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeMemoryHelpers
{

    private unsafe static delegate* unmanaged<byte*, nint> _FetchInterfaceByName;

    /// <summary>
    /// supports both internal interface system, but also valve interface system
    /// </summary>
    public unsafe static nint FetchInterfaceByName(string ifaceName)
    {
        return StringAlloc.CreateCString(ifaceName, ifaceNameBufferPtr =>
        {
            var ret = _FetchInterfaceByName((byte*)ifaceNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, nint> _GetVirtualTableAddress;

    public unsafe static nint GetVirtualTableAddress(string library, string vtableName)
    {
        return StringAlloc.CreateCString(library, libraryBufferPtr =>
        {
            return StringAlloc.CreateCString(vtableName, vtableNameBufferPtr =>
            {
                var ret = _GetVirtualTableAddress((byte*)libraryBufferPtr, (byte*)vtableNameBufferPtr);
                return ret;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, byte*, nint> _GetVirtualTableAddressNested2;

    public unsafe static nint GetVirtualTableAddressNested2(string library, string class1, string class2)
    {
        return StringAlloc.CreateCString(library, libraryBufferPtr =>
        {
            return StringAlloc.CreateCString(class1, class1BufferPtr =>
            {
                return StringAlloc.CreateCString(class2, class2BufferPtr =>
                {
                    var ret = _GetVirtualTableAddressNested2((byte*)libraryBufferPtr, (byte*)class1BufferPtr, (byte*)class2BufferPtr);
                    return ret;
                });
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, byte*, int, byte, nint> _GetAddressBySignature;

    public unsafe static nint GetAddressBySignature(string library, string sig, int len, bool rawBytes)
    {
        return StringAlloc.CreateCString(library, libraryBufferPtr =>
        {
            return StringAlloc.CreateCString(sig, sigBufferPtr =>
            {
                var ret = _GetAddressBySignature((byte*)libraryBufferPtr, (byte*)sigBufferPtr, len, rawBytes ? (byte)1 : (byte)0);
                return ret;
            });
        });
    }

    private unsafe static delegate* unmanaged<int*, nint, byte*> _GetObjectPtrVtableName;

    public unsafe static string GetObjectPtrVtableName(nint objptr)
    {
        var length = 0;
        var returnedPtr = _GetObjectPtrVtableName(&length, objptr);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<nint, byte> _ObjectPtrHasVtable;

    public unsafe static bool ObjectPtrHasVtable(nint objptr)
    {
        var ret = _ObjectPtrHasVtable(objptr);
        return ret == 1;
    }

    private unsafe static delegate* unmanaged<nint, byte*, byte> _ObjectPtrHasBaseClass;

    public unsafe static bool ObjectPtrHasBaseClass(nint objptr, string baseClassName)
    {
        return StringAlloc.CreateCString(baseClassName, baseClassNameBufferPtr =>
        {
            var ret = _ObjectPtrHasBaseClass(objptr, (byte*)baseClassNameBufferPtr);
            return ret == 1;
        });
    }
}