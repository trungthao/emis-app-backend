using MediatR;
using Message.Application.DTOs;
using Message.Domain.Entities;
using Message.Domain.Repositories;
using Message.Domain.Enums;

namespace Message.Application.Commands;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;

    public CreateConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra nếu là Direct Message, có conversation tồn tại chưa
        if (request.Type == ConversationType.DirectMessage && request.MemberIds.Count == 2)
        {
            var existing = await _conversationRepository.FindDirectConversationAsync(
                request.MemberIds[0], 
                request.MemberIds[1], 
                cancellationToken);
            
            if (existing != null)
            {
                return MapToDto(existing, request.CreatedBy);
            }
        }

        // Kiểm tra nếu là StudentGroup, có conversation tồn tại chưa
        if (request.Type == ConversationType.StudentGroup && request.StudentId.HasValue)
        {
            var existing = await _conversationRepository.FindStudentGroupAsync(
                request.StudentId.Value, 
                cancellationToken);
            
            if (existing != null)
            {
                return MapToDto(existing, request.CreatedBy);
            }
        }

        // Tạo conversation mới
        var conversation = new Conversation
        {
            Type = request.Type,
            GroupName = request.GroupName,
            StudentId = request.StudentId,
            ClassId = request.ClassId,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            Members = request.MemberIds.Select((memberId, index) => new ConversationMember
            {
                UserId = memberId,
                Role = index == 0 && request.Type == ConversationType.CustomGroup 
                    ? MemberRole.Owner 
                    : MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            }).ToList()
        };

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);
        
        return MapToDto(created, request.CreatedBy);
    }

    private ConversationDto MapToDto(Conversation conversation, string currentUserId)
    {
        var currentMember = conversation.Members.FirstOrDefault(m => m.UserId == currentUserId);
        
        return new ConversationDto
        {
            Id = conversation.Id,
            Type = conversation.Type,
            GroupName = conversation.GroupName,
            GroupAvatar = conversation.GroupAvatar,
            StudentId = conversation.StudentId,
            ClassId = conversation.ClassId,
            Members = conversation.Members.Select(m => new ConversationMemberDto
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
            LastMessage = conversation.LastMessage != null ? new LastMessageDto
            {
                Content = conversation.LastMessage.Content,
                SenderId = conversation.LastMessage.SenderId,
                SenderName = conversation.LastMessage.SenderName,
                SentAt = conversation.LastMessage.SentAt,
                HasAttachment = conversation.LastMessage.HasAttachment
            } : null,
            TotalMessages = conversation.TotalMessages,
            UnreadCount = currentMember?.UnreadCount ?? 0,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }
}
