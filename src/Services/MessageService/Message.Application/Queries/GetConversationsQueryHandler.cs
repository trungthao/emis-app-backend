using MediatR;
using Message.Application.DTOs;
using Message.Domain.Repositories;

namespace Message.Application.Queries;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationsQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(
            request.UserId,
            request.Skip,
            request.Limit,
            cancellationToken);

        return conversations.Select(c =>
        {
            var currentMember = c.Members.FirstOrDefault(m => m.UserId == request.UserId);

            return new ConversationDto
            {
                Id = c.Id,
                Type = c.Type,
                GroupName = c.GroupName,
                GroupAvatar = c.GroupAvatar,
                StudentId = c.StudentId,
                ClassId = c.ClassId,
                Members = c.Members.Select(m => new ConversationMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.UserName,
                    Avatar = m.Avatar,
                    UserType = m.UserType,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    LastSeenAt = m.LastSeenAt,
                    UnreadCount = m.UnreadCount,
                    IsMuted = m.IsMuted
                }).ToList(),
                LastMessage = c.LastMessage != null ? new LastMessageDto
                {
                    Content = c.LastMessage.Content,
                    SenderId = c.LastMessage.SenderId,
                    SenderName = c.LastMessage.SenderName,
                    SentAt = c.LastMessage.SentAt,
                    HasAttachment = c.LastMessage.HasAttachment
                } : null,
                TotalMessages = c.TotalMessages,
                UnreadCount = currentMember?.UnreadCount ?? 0,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }).ToList();
    }
}
