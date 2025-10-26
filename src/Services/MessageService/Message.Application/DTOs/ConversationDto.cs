using Message.Domain.Enums;

namespace Message.Application.DTOs;

/// <summary>
/// DTO cho Conversation
/// </summary>
public class ConversationDto
{
    public string Id { get; set; } = string.Empty;
    public ConversationType Type { get; set; }
    public string? GroupName { get; set; }
    public string? GroupAvatar { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = new();
    public LastMessageDto? LastMessage { get; set; }
    public int TotalMessages { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ConversationMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public UserType UserType { get; set; }
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
}

public class LastMessageDto
{
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool HasAttachment { get; set; }
}
