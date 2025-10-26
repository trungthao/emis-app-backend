using EMIS.EventBus.Abstractions;

namespace EMIS.Contracts.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class BaseEvent : IEvent
{
    protected BaseEvent()
    {
        EventId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public Guid EventId { get; init; }

    /// <inheritdoc />
    public DateTime Timestamp { get; init; }

    /// <inheritdoc />
    public abstract string EventType { get; }
}
