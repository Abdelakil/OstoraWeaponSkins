#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeKeyValuesSystem
{

    private unsafe static delegate* unmanaged<byte*, uint> _GetSymbolForString;

    public unsafe static uint GetSymbolForString(string str)
    {
        return StringAlloc.CreateCString(str, strBufferPtr =>
        {
            var ret = _GetSymbolForString((byte*)strBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<int*, uint, byte*> _GetStringForSymbol;

    public unsafe static string GetStringForSymbol(uint symbol)
    {
        var length = 0;
        var returnedPtr = _GetStringForSymbol(&length, symbol);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }
}