using System;
using System.Runtime.InteropServices;

namespace SwiftlyS2.Shared.Natives;

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct CAnimGraph2ParamOptionalRefFloat
{
    [FieldOffset(0x00)] private CGlobalSymbol _name;
    [FieldOffset(0x08)] private nint _pManager;
    [FieldOffset(0x10)] private short _index;
    [FieldOffset(0x13)] private bool _bound;

    public string Name => _name.Value;
    public short Index => _index;
    public bool IsBound => _bound;
    public bool IsValid => _bound && _index >= 0 && _pManager != 0;

    private float Param(float? value = null)
    {
        unsafe
        {
            var arr = *(nint*)(_pManager + 0x10);
            var addr = *(nint*)(arr + (nint)_index * 8);
            if (value is float v)
            {
                *(float*)(addr + 0x18) = v;
                return v;
            }
            return *(float*)(addr + 0x18);
        }
    }

    public float Value {
        get => IsValid ? Param() : default;
        set { if (IsValid) _ = Param(value); }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public unsafe struct CAnimGraph2ParamOptionalRefBool
{
    [FieldOffset(0x00)] private CGlobalSymbol _name;
    [FieldOffset(0x08)] private nint _pManager;
    [FieldOffset(0x10)] private short _index;
    [FieldOffset(0x13)] private bool _bound;

    public string Name => _name.Value;
    public short Index => _index;
    public bool IsBound => _bound;
    public bool IsValid => _bound && _index >= 0 && _pManager != 0;

    private byte Param(byte? value = null)
    {
        unsafe
        {
            var arr = *(nint*)(_pManager + 0x10);
            var addr = *(nint*)(arr + (nint)_index * 8);
            if (value is byte v)
            {
                *(byte*)(addr + 0x18) = v;
                return v;
            }
            return *(byte*)(addr + 0x18);
        }
    }

    public bool Value {
        get => IsValid && Param() == 1;
        set { if (IsValid) _ = Param(Convert.ToByte(value)); }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public unsafe struct CAnimGraph2ParamOptionalRefCGlobalSymbol
{
    [FieldOffset(0x00)] private CGlobalSymbol _name;
    [FieldOffset(0x08)] private nint _pManager;
    [FieldOffset(0x10)] private short _index;
    [FieldOffset(0x13)] private bool _bound;

    public string Name => _name.Value;
    public short Index => _index;
    public bool IsBound => _bound;
    public bool IsValid => _bound && _index >= 0 && _pManager != 0;

    private string Param(string? value = null)
    {
        unsafe
        {
            var arr = *(nint*)(_pManager + 0x10);
            var addr = *(nint*)(arr + (nint)_index * 8);
            if (value is string v)
            {
                (*(CGlobalSymbol*)(addr + 0x18)).Value = value;
                return v;
            }
            return (*(CGlobalSymbol*)(addr + 0x18)).Value;
        }
    }

    public string Value {
        get => IsValid ? Param() : default;
        set { if (IsValid) _ = Param(value); }
    }
}