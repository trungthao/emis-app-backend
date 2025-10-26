using MediatR;
using Message.Application.DTOs;
using Message.Domain.Repositories;

namespace Message.Application.Queries;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;

    public GetMessagesQueryHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<List<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _messageRepository.GetByConversationIdAsync(
            request.ConversationId,
            request.Skip,
            request.Limit,
            cancellationToken);

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.SenderName,
            SenderType = m.SenderType,
            Content = m.Content,
            Status = m.Status,
            Attachments = m.Attachments.Select(a => new MessageAttachmentDto
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                FileSize = a.FileSize,
                ThumbnailUrl = a.ThumbnailUrl
            }).ToList(),
            ReplyToMessageId = m.ReplyToMessageId,
            ReplyToContent = m.ReplyToContent,
            ReadBy = m.ReadBy.Select(r => new MessageReadStatusDto
            {
                UserId = r.UserId,
                ReadAt = r.ReadAt
            }).ToList(),
            SentAt = m.SentAt,
            EditedAt = m.EditedAt,
            IsDeleted = m.IsDeleted
        }).ToList();
    }
}
