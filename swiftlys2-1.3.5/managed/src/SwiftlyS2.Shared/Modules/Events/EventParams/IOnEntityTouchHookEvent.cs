using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Shared.Events;

/// <summary>
/// Called when an entity starts touching another entity.
/// </summary>
public interface IOnEntityStartTouchEvent
{

    /// <summary>
    /// Gets the entity that initiated the touch.
    /// </summary>
    public CBaseEntity Entity { get; }

    /// <summary>
    /// Gets the entity being touched.
    /// </summary>
    public CBaseEntity OtherEntity { get; }
}

/// <summary>
/// Called when an entity is touching another entity.
/// </summary>
public interface IOnEntityTouchEvent
{

    /// <summary>
    /// Gets the entity that initiated the touch.
    /// </summary>
    public CBaseEntity Entity { get; }

    /// <summary>
    /// Gets the entity being touched.
    /// </summary>
    public CBaseEntity OtherEntity { get; }
}

/// <summary>
/// Called when an entity ends touching another entity.
/// </summary>
public interface IOnEntityEndTouchEvent
{

    /// <summary>
    /// Gets the entity that initiated the touch.
    /// </summary>
    public CBaseEntity Entity { get; }

    /// <summary>
    /// Gets the entity being touched.
    /// </summary>
    public CBaseEntity OtherEntity { get; }
}