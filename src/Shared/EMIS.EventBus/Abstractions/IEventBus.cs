namespace EMIS.EventBus.Abstractions;

/// <summary>
/// Event bus interface for publishing and subscribing to events
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an event to the event bus
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    /// <param name="event">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribe to an event type
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    /// <typeparam name="TEventHandler">Type of event handler</typeparam>
    void Subscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>;

    /// <summary>
    /// Unsubscribe from an event type
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    /// <typeparam name="TEventHandler">Type of event handler</typeparam>
    void Unsubscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>;
}
