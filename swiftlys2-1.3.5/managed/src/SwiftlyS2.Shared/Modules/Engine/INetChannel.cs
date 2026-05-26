namespace SwiftlyS2.Shared.Engine;

public interface INetChannel : INetChannelInfo
{
    public bool CanPacket { get; }
    public bool IsOverflowed { get; }
    public bool HasPendingReliableData { get; }

    public void SetMinDataRate( int rate );
    public void SetMaxDataRate( int rate );
    public void SetTimeout( float seconds, bool forceExact = false );
    public bool IsTimedOut { get; }
    public void SuppressTransmit( bool suppress );
    public bool IsSuppressingTransmit { get; }
}