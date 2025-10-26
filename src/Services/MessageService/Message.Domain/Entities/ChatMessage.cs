using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Message.Domain.Enums;

namespace Message.Domain.Entities;

/// <summary>
/// Tin nhắn (Message)
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// ID của tin nhắn (MongoDB ObjectId)
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// ID cuộc hội thoại
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
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
    /// Loại người gửi
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public UserType SenderType { get; set; }
    
    /// <summary>
    /// Nội dung tin nhắn
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Trạng thái tin nhắn
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    
    /// <summary>
    /// File đính kèm
    /// </summary>
    public List<MessageAttachment> Attachments { get; set; } = new();
    
    /// <summary>
    /// ID tin nhắn được trả lời
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReplyToMessageId { get; set; }
    
    /// <summary>
    /// Nội dung tin nhắn được trả lời (để hiển thị)
    /// </summary>
    public string? ReplyToContent { get; set; }
    
    /// <summary>
    /// Danh sách người đã đọc tin nhắn
    /// </summary>
    public List<MessageReadStatus> ReadBy { get; set; } = new();
    
    /// <summary>
    /// Ngày gửi
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Ngày chỉnh sửa
    /// </summary>
    public DateTime? EditedAt { get; set; }
    
    /// <summary>
    /// Có bị xóa không
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Ngày xóa
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// File đính kèm trong tin nhắn
/// </summary>
public class MessageAttachment
{
    /// <summary>
    /// Tên file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL file
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại file (image, document, video, etc.)
    /// </summary>
    public string FileType { get; set; } = string.Empty;
    
    /// <summary>
    /// Kích thước file (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Thumbnail URL (cho ảnh, video)
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Trạng thái đã đọc của tin nhắn
/// </summary>
public class MessageReadStatus
{
    /// <summary>
    /// ID người đọc
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian đọc
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
