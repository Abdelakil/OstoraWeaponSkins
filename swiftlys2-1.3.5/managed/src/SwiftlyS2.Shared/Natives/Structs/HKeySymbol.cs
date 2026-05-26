using System.Runtime.InteropServices;
using SwiftlyS2.Core.Natives;

namespace SwiftlyS2.Shared.Natives;

[StructLayout(LayoutKind.Sequential)]
public struct HKeySymbol
{
    public uint Hash;

    public string Value {
        get => NativeKeyValuesSystem.GetStringForSymbol(Hash);
        set => Hash = NativeKeyValuesSystem.GetSymbolForString(value);
    }
}