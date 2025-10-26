using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Message.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Message.API.EventHandlers;

/// <summary>
/// Event handler để broadcast tin nhắn qua SignalR
/// Nhận event từ Kafka và gửi đến tất cả clients qua SignalR
/// </summary>
public class MessageSentEventHandler : IEventHandler<MessageSentEvent>
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<MessageSentEventHandler> _logger;

    public MessageSentEventHandler(
        IHubContext<ChatHub> hubContext,
        ILogger<MessageSentEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task HandleAsync(MessageSentEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Broadcasting message {MessageId} to conversation {ConversationId} via SignalR",
                @event.MessageId,
                @event.ConversationId);

            // Broadcast tin nhắn đến tất cả members trong conversation
            await _hubContext.Clients
                .Group(@event.ConversationId)
                .SendAsync("ReceiveMessage", @event.MessageData, cancellationToken);

            _logger.LogInformation(
                "Successfully broadcasted message {MessageId} to conversation {ConversationId}",
                @event.MessageId,
                @event.ConversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to broadcast message {MessageId} to conversation {ConversationId}",
                @event.MessageId,
                @event.ConversationId);

            // Không throw - để Kafka có thể retry
            throw;
        }
    }
}
