using MediatR;
using Message.Application.DTOs;
using Message.Domain.Enums;

namespace Message.Application.Commands;

/// <summary>
/// Command để tạo conversation mới
/// </summary>
public record CreateConversationCommand : IRequest<ConversationDto>
{
    public ConversationType Type { get; init; }
    public string? GroupName { get; init; }
    public Guid? StudentId { get; init; }
    public Guid? ClassId { get; init; }
    public List<string> MemberIds { get; init; } = new();
    public string CreatedBy { get; init; } = string.Empty;
}
