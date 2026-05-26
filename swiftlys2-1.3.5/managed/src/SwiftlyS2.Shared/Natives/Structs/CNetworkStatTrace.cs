using System.Runtime.InteropServices;

namespace SwiftlyS2.Shared.Natives;

[StructLayout(LayoutKind.Sequential)]
public struct Spike_t
{
    public CUtlString Desc;
    public int Bits;
}

[StructLayout(LayoutKind.Sequential)]
public struct CNetworkStatTrace
{
    public CUtlVector<Spike_t> Records;
    public int MinWarningBytes;
    public int StartBit;
    public int CurrentBit;
}