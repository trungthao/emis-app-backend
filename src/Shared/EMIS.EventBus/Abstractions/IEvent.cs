namespace EMIS.EventBus.Abstractions;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Type of the event (used for routing and deserialization)
    /// </summary>
    string EventType { get; }
}
