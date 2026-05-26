#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeCommandLine
{

    private unsafe static delegate* unmanaged<byte*, byte> _HasParameter;

    public unsafe static bool HasParameter(string parameter)
    {
        return StringAlloc.CreateCString(parameter, parameterBufferPtr =>
        {
            var ret = _HasParameter((byte*)parameterBufferPtr);
            return ret == 1;
        });
    }

    private unsafe static delegate* unmanaged<int> _GetParameterCount;

    public unsafe static int GetParameterCount()
    {
        var ret = _GetParameterCount();
        return ret;
    }

    private unsafe static delegate* unmanaged<int*, byte*, byte*, byte*> _GetParameterValueString;

    public unsafe static string GetParameterValueString(string parameter, string defaultValue)
    {
        return StringAlloc.CreateCString(parameter, parameterBufferPtr =>
        {
            return StringAlloc.CreateCString(defaultValue, defaultValueBufferPtr =>
            {
                var length = 0;
                var returnedPtr = _GetParameterValueString(&length, (byte*)parameterBufferPtr, (byte*)defaultValueBufferPtr);
                var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
                NativeAllocator.Free((nint)returnedPtr);
                return outString;
            });
        });
    }

    private unsafe static delegate* unmanaged<byte*, int, int> _GetParameterValueInt;

    public unsafe static int GetParameterValueInt(string parameter, int defaultValue)
    {
        return StringAlloc.CreateCString(parameter, parameterBufferPtr =>
        {
            var ret = _GetParameterValueInt((byte*)parameterBufferPtr, defaultValue);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<byte*, float, float> _GetParameterValueFloat;

    public unsafe static float GetParameterValueFloat(string parameter, float defaultValue)
    {
        return StringAlloc.CreateCString(parameter, parameterBufferPtr =>
        {
            var ret = _GetParameterValueFloat((byte*)parameterBufferPtr, defaultValue);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<int*, byte*> _GetCommandLine;

    public unsafe static string GetCommandLine()
    {
        var length = 0;
        var returnedPtr = _GetCommandLine(&length);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<byte> _HasParameters;

    public unsafe static bool HasParameters()
    {
        var ret = _HasParameters();
        return ret == 1;
    }
}