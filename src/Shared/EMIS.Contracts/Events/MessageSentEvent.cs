using EMIS.Contracts.Events;

namespace EMIS.Contracts.Events;

/// <summary>
/// Event khi có tin nhắn mới được gửi
/// Được publish để broadcast qua SignalR
/// </summary>
public class MessageSentEvent : BaseEvent
{
    public override string EventType => nameof(MessageSentEvent);

    /// <summary>
    /// ID của tin nhắn
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// ID của conversation
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// ID người gửi
    /// </summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Tên người gửi
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung tin nhắn
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Có file đính kèm không
    /// </summary>
    public bool HasAttachment { get; set; }

    /// <summary>
    /// Số lượng file đính kèm
    /// </summary>
    public int AttachmentCount { get; set; }

    /// <summary>
    /// ID tin nhắn được reply (nếu có)
    /// </summary>
    public string? ReplyToMessageId { get; set; }

    /// <summary>
    /// Thời gian gửi
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Full message DTO để broadcast
    /// </summary>
    public object MessageData { get; set; } = new();
}
