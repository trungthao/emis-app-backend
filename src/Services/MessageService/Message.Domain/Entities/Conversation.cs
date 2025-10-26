using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Message.Domain.Enums;

namespace Message.Domain.Entities;

/// <summary>
/// Cuộc hội thoại (Conversation)
/// </summary>
public class Conversation
{
    /// <summary>
    /// ID của conversation (MongoDB ObjectId)
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Loại cuộc hội thoại
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public ConversationType Type { get; set; }
    
    /// <summary>
    /// Tên nhóm chat (chỉ áp dụng cho CustomGroup)
    /// </summary>
    public string? GroupName { get; set; }
    
    /// <summary>
    /// Avatar của nhóm (URL)
    /// </summary>
    public string? GroupAvatar { get; set; }
    
    /// <summary>
    /// ID của học sinh (dùng cho StudentGroup)
    /// </summary>
    public Guid? StudentId { get; set; }
    
    /// <summary>
    /// ID của lớp học (dùng cho StudentGroup)
    /// </summary>
    public Guid? ClassId { get; set; }
    
    /// <summary>
    /// Danh sách thành viên
    /// </summary>
    public List<ConversationMember> Members { get; set; } = new();
    
    /// <summary>
    /// Tin nhắn cuối cùng
    /// </summary>
    public LastMessage? LastMessage { get; set; }
    
    /// <summary>
    /// Tổng số tin nhắn
    /// </summary>
    public int TotalMessages { get; set; }
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Người tạo
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Ngày cập nhật cuối
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Có bị xóa không (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Ngày xóa
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Người xóa
    /// </summary>
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Thông tin tin nhắn cuối cùng trong conversation
/// </summary>
public class LastMessage
{
    /// <summary>
    /// Nội dung tin nhắn
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// ID người gửi
    /// </summary>
    public string SenderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên người gửi
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Thời gian gửi
    /// </summary>
    public DateTime SentAt { get; set; }
    
    /// <summary>
    /// Có file đính kèm không
    /// </summary>
    public bool HasAttachment { get; set; }
}
