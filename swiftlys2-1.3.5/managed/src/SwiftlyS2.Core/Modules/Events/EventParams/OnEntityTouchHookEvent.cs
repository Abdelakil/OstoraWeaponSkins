using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.Events;

internal class OnEntityStartTouchEvent : IOnEntityStartTouchEvent, IDisposable
{
  public required CBaseEntity Entity { get; init; }

  public required CBaseEntity OtherEntity { get; init; }

  public void Dispose()
  {
  }
}

internal class OnEntityTouchEvent : IOnEntityTouchEvent, IDisposable
{
  public required CBaseEntity Entity { get; init; }

  public required CBaseEntity OtherEntity { get; init; }

  public void Dispose()
  {
  }
}

internal class OnEntityEndTouchEvent : IOnEntityEndTouchEvent, IDisposable
{
  public required CBaseEntity Entity { get; init; }

  public required CBaseEntity OtherEntity { get; init; }

  public void Dispose()
  {
  }
}