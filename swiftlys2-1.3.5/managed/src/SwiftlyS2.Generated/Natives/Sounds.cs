#pragma warning disable CS0649
#pragma warning disable CS0169

using System.Buffers;
using System.Text;
using System.Threading;
using SwiftlyS2.Shared.Natives;

namespace SwiftlyS2.Core.Natives;

internal static class NativeSounds
{

    private unsafe static delegate* unmanaged<nint> _CreateSoundEvent;

    public unsafe static nint CreateSoundEvent()
    {
        var ret = _CreateSoundEvent();
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, void> _DestroySoundEvent;

    public unsafe static void DestroySoundEvent(nint soundEvent)
    {
        _DestroySoundEvent(soundEvent);
    }

    private unsafe static delegate* unmanaged<nint, uint> _Emit;

    public unsafe static uint Emit(nint soundEvent)
    {
        if (!NativeBinding.IsMainThread)
        {
            throw new InvalidOperationException("This method can only be called from the main thread.");
        }
        var ret = _Emit(soundEvent);
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, byte*, void> _SetName;

    public unsafe static void SetName(nint soundEvent, string name)
    {
        StringAlloc.CreateCString(name, nameBufferPtr =>
        {
            _SetName(soundEvent, (byte*)nameBufferPtr);
        });
    }

    private unsafe static delegate* unmanaged<int*, nint, byte*> _GetName;

    public unsafe static string GetName(nint soundEvent)
    {
        var length = 0;
        var returnedPtr = _GetName(&length, soundEvent);
        var outString = StringAlloc.CreateCSharpString((nint)returnedPtr, length);
        NativeAllocator.Free((nint)returnedPtr);
        return outString;
    }

    private unsafe static delegate* unmanaged<nint, int, void> _SetSourceEntityIndex;

    public unsafe static void SetSourceEntityIndex(nint soundEvent, int index)
    {
        _SetSourceEntityIndex(soundEvent, index);
    }

    private unsafe static delegate* unmanaged<nint, int> _GetSourceEntityIndex;

    public unsafe static int GetSourceEntityIndex(nint soundEvent)
    {
        var ret = _GetSourceEntityIndex(soundEvent);
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, int, void> _AddClient;

    public unsafe static void AddClient(nint soundEvent, int playerid)
    {
        _AddClient(soundEvent, playerid);
    }

    private unsafe static delegate* unmanaged<nint, int, void> _RemoveClient;

    public unsafe static void RemoveClient(nint soundEvent, int playerid)
    {
        _RemoveClient(soundEvent, playerid);
    }

    private unsafe static delegate* unmanaged<nint, void> _ClearClients;

    public unsafe static void ClearClients(nint soundEvent)
    {
        _ClearClients(soundEvent);
    }

    private unsafe static delegate* unmanaged<nint, void> _AddAllClients;

    public unsafe static void AddAllClients(nint soundEvent)
    {
        _AddAllClients(soundEvent);
    }

    private unsafe static delegate* unmanaged<nint, byte*, byte> _HasField;

    public unsafe static bool HasField(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _HasField(soundEvent, (byte*)fieldNameBufferPtr);
            return ret == 1;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, byte, void> _SetBool;

    public unsafe static void SetBool(nint soundEvent, string fieldName, bool value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetBool(soundEvent, (byte*)fieldNameBufferPtr, value ? (byte)1 : (byte)0);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, byte> _GetBool;

    public unsafe static bool GetBool(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetBool(soundEvent, (byte*)fieldNameBufferPtr);
            return ret == 1;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, int, void> _SetInt32;

    public unsafe static void SetInt32(nint soundEvent, string fieldName, int value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetInt32(soundEvent, (byte*)fieldNameBufferPtr, value);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, int> _GetInt32;

    public unsafe static int GetInt32(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetInt32(soundEvent, (byte*)fieldNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, uint, void> _SetUInt32;

    public unsafe static void SetUInt32(nint soundEvent, string fieldName, uint value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetUInt32(soundEvent, (byte*)fieldNameBufferPtr, value);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, uint> _GetUInt32;

    public unsafe static uint GetUInt32(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetUInt32(soundEvent, (byte*)fieldNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, ulong, void> _SetUInt64;

    public unsafe static void SetUInt64(nint soundEvent, string fieldName, ulong value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetUInt64(soundEvent, (byte*)fieldNameBufferPtr, value);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, ulong> _GetUInt64;

    public unsafe static ulong GetUInt64(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetUInt64(soundEvent, (byte*)fieldNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, float, void> _SetFloat;

    public unsafe static void SetFloat(nint soundEvent, string fieldName, float value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetFloat(soundEvent, (byte*)fieldNameBufferPtr, value);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, float> _GetFloat;

    public unsafe static float GetFloat(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetFloat(soundEvent, (byte*)fieldNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, Vector, void> _SetFloat3;

    public unsafe static void SetFloat3(nint soundEvent, string fieldName, Vector value)
    {
        StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            _SetFloat3(soundEvent, (byte*)fieldNameBufferPtr, value);
        });
    }

    private unsafe static delegate* unmanaged<nint, byte*, Vector> _GetFloat3;

    public unsafe static Vector GetFloat3(nint soundEvent, string fieldName)
    {
        return StringAlloc.CreateCString(fieldName, fieldNameBufferPtr =>
        {
            var ret = _GetFloat3(soundEvent, (byte*)fieldNameBufferPtr);
            return ret;
        });
    }

    private unsafe static delegate* unmanaged<nint, ulong> _GetClients;

    /// <summary>
    /// returns player mask
    /// </summary>
    public unsafe static ulong GetClients(nint soundEvent)
    {
        var ret = _GetClients(soundEvent);
        return ret;
    }

    private unsafe static delegate* unmanaged<nint, ulong, void> _SetClients;

    public unsafe static void SetClients(nint soundEvent, ulong playermask)
    {
        _SetClients(soundEvent, playermask);
    }
}