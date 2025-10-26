using MediatR;
using Message.Application.DTOs;

namespace Message.Application.Queries;

/// <summary>
/// Query để lấy messages của một conversation
/// </summary>
public record GetMessagesQuery : IRequest<List<MessageDto>>
{
    public string ConversationId { get; init; } = string.Empty;
    public int Skip { get; init; } = 0;
    public int Limit { get; init; } = 50;
}
