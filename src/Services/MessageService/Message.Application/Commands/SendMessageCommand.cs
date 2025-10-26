using MediatR;
using Message.Application.DTOs;

namespace Message.Application.Commands;

/// <summary>
/// Command để gửi tin nhắn
/// </summary>
public record SendMessageCommand : IRequest<MessageDto>
{
    public string ConversationId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? ReplyToMessageId { get; init; }
    public List<MessageAttachmentDto> Attachments { get; init; } = new();
}
