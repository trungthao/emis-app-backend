using MediatR;
using Message.Application.DTOs;

namespace Message.Application.Queries;

/// <summary>
/// Query để lấy danh sách conversations của user
/// </summary>
public record GetConversationsQuery : IRequest<List<ConversationDto>>
{
    public string UserId { get; init; } = string.Empty;
    public int Skip { get; init; } = 0;
    public int Limit { get; init; } = 50;
}
