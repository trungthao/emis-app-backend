using Message.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Message.Domain.Entities;

/// <summary>
/// Thành viên của cuộc hội thoại
/// </summary>
public class ConversationMember
{
    /// <summary>
    /// ID người dùng (từ Auth Service)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Tên người dùng
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar URL
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Loại người dùng
    /// </summary>
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public UserType UserType { get; set; }

    /// <summary>
    /// Vai trò trong nhóm
    /// </summary>
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public MemberRole Role { get; set; }

    /// <summary>
    /// Ngày tham gia
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lần xem cuối cùng
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// Số tin nhắn chưa đọc
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Có tắt thông báo không
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// Đã rời nhóm chưa
    /// </summary>
    public bool HasLeft { get; set; }

    /// <summary>
    /// Ngày rời nhóm
    /// </summary>
    public DateTime? LeftAt { get; set; }
}
