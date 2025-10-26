using Message.Domain.Enums;

namespace Message.Application.DTOs;

/// <summary>
/// DTO cho Message
/// </summary>
public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public UserType SenderType { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageStatus Status { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; } = new();
    public string? ReplyToMessageId { get; set; }
    public string? ReplyToContent { get; set; }
    public List<MessageReadStatusDto> ReadBy { get; set; } = new();
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class MessageAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class MessageReadStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; }
}
