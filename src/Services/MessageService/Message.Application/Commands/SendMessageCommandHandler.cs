using MediatR;
using Message.Application.DTOs;
using Message.Domain.Repositories;
using Message.Domain.Enums;
using EMIS.EventBus.Abstractions;
using EMIS.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Message.Application.Commands;

/// <summary>
/// Write-Behind Pattern: Chỉ publish event lên Kafka, không write trực tiếp vào MongoDB
/// Consumer sẽ batch write để tăng throughput
/// </summary>
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IEventBus eventBus,
        ILogger<SendMessageCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // ✅ VALIDATION: Kiểm tra conversation tồn tại
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation không tồn tại");
        }

        // ✅ VALIDATION: Kiểm tra sender có trong conversation không
        var sender = conversation.Members.FirstOrDefault(m => m.UserId == request.SenderId);
        if (sender == null)
        {
            throw new InvalidOperationException("Bạn không phải là thành viên của cuộc hội thoại này");
        }

        // 🔥 WRITE-BEHIND PATTERN: Publish event to Kafka (KHÔNG save to MongoDB ngay)
        // Consumer sẽ batch write để tăng throughput
        
        var temporaryMessageId = Guid.NewGuid().ToString(); // Temporary ID
        var correlationId = Guid.NewGuid().ToString(); // Tracking ID
        
        var sendMessageRequestedEvent = new SendMessageRequestedEvent
        {
            TemporaryMessageId = temporaryMessageId,
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderType = (EMIS.Contracts.Enums.UserType)sender.UserType, // Cast from Domain enum to Contracts enum
            Content = request.Content,
            Attachments = request.Attachments.Select(a => new MessageAttachmentData
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                FileSize = a.FileSize
            }).ToList(),
            ReplyToMessageId = request.ReplyToMessageId,
            RequestedAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        try
        {
            // Publish to Kafka (fast ~1-2ms)
            await _eventBus.PublishAsync(sendMessageRequestedEvent, cancellationToken);
            
            _logger.LogInformation(
                "📤 Published SendMessageRequestedEvent: TempId={TempId}, ConversationId={ConversationId}, CorrelationId={CorrelationId}",
                temporaryMessageId,
                request.ConversationId,
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to publish SendMessageRequestedEvent for ConversationId={ConversationId}",
                request.ConversationId);
            throw; // Throw để client biết request failed
        }

        // ✅ Return DTO với temporary data (message chưa được persist to MongoDB)
        // API sẽ return 202 Accepted thay vì 201 Created
        return new MessageDto
        {
            Id = temporaryMessageId, // Temporary ID - sẽ thay bằng MongoDB ID sau
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderName = request.SenderName,
            SenderType = sender.UserType,
            Content = request.Content,
            Status = MessageStatus.Sent, // Sent (will be persisted asynchronously)
            Attachments = request.Attachments.Select(a => new MessageAttachmentDto
            {
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                FileSize = a.FileSize,
                ThumbnailUrl = a.ThumbnailUrl
            }).ToList(),
            ReplyToMessageId = request.ReplyToMessageId,
            ReadBy = new List<MessageReadStatusDto>(),
            SentAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }
}
