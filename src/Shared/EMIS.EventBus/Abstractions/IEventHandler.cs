namespace EMIS.EventBus.Abstractions;

/// <summary>
/// Interface for event handlers
/// </summary>
/// <typeparam name="TEvent">Type of event to handle</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : class, IEvent
{
    /// <summary>
    /// Handle the event
    /// </summary>
    /// <param name="event">Event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
