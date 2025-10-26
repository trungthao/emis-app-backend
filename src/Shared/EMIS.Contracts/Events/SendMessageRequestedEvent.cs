using EMIS.Contracts.Enums;
using EMIS.EventBus.Abstractions;

namespace EMIS.Contracts.Events;

/// <summary>
/// Event được publish khi client request gửi tin nhắn (BEFORE save to MongoDB)
/// Sử dụng Write-Behind Pattern: API publish event → Kafka → Consumer batch write to DB
/// </summary>
public class SendMessageRequestedEvent : BaseEvent
{
    public override string EventType => nameof(SendMessageRequestedEvent);

    /// <summary>
    /// Temporary ID được generate client-side hoặc API-side để tracking
    /// Sẽ được thay thế bằng MongoDB ObjectId sau khi save
    /// </summary>
    public string TemporaryMessageId { get; set; } = string.Empty;

    public string ConversationId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public UserType SenderType { get; set; }
    public string Content { get; set; } = string.Empty;

    public List<MessageAttachmentData> Attachments { get; set; } = new();
    public string? ReplyToMessageId { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Client correlation ID để track request từ client → Kafka → DB
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Attachment data trong event (trước khi save to MongoDB)
/// </summary>
public class MessageAttachmentData
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
