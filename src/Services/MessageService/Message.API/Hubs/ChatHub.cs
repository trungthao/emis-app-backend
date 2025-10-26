using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Message.API.Hubs;

/// <summary>
/// SignalR Hub cho real-time messaging
/// Hub này chỉ xử lý:
/// - Join/Leave conversation
/// - Typing indicators
/// - Online/Offline status
/// - Mark as read
/// 
/// TIN NHẮN THỰC TẾ được gửi qua REST API → Kafka → MessageSentEventHandler → Broadcast
/// </summary>
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join conversation (vào phòng chat)
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        _logger.LogInformation(
            "User {UserId} joined conversation {ConversationId}",
            GetUserId(),
            conversationId);
    }

    /// <summary>
    /// Leave conversation (rời phòng chat)
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        _logger.LogInformation(
            "User {UserId} left conversation {ConversationId}",
            GetUserId(),
            conversationId);
    }

    /// <summary>
    /// Typing indicator (đang gõ)
    /// </summary>
    public async Task Typing(string conversationId, string userName)
    {
        await Clients.OthersInGroup(conversationId).SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserName = userName,
            IsTyping = true
        });
    }

    /// <summary>
    /// Stop typing indicator (ngừng gõ)
    /// </summary>
    public async Task StopTyping(string conversationId, string userName)
    {
        await Clients.OthersInGroup(conversationId).SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserName = userName,
            IsTyping = false
        });
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    public async Task MarkAsRead(string conversationId, string messageId)
    {
        var userId = GetUserId();

        // TODO: Call repository để mark message as read

        // Notify other users
        await Clients.OthersInGroup(conversationId).SendAsync("MessageRead", new
        {
            ConversationId = conversationId,
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// User online status
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}",
            userId, Context.ConnectionId);

        // Notify others that user is online
        await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// User offline status
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        _logger.LogInformation("User {UserId} disconnected", userId);

        // Notify others that user is offline
        await Clients.Others.SendAsync("UserOffline", userId);

        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId()
    {
        // Lấy userId từ claim trong JWT token
        return Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("userId")?.Value
            ?? Context.ConnectionId;
    }
}
