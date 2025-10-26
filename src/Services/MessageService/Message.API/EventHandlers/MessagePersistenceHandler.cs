using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Message.Domain.Entities;
using Message.Domain.Repositories;
using Message.Domain.Enums;
using Message.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Message.API.EventHandlers;

/// <summary>
/// Write-Behind Pattern: Consumer để batch write messages to MongoDB
/// Tăng throughput bằng cách gom nhiều messages write cùng lúc
/// </summary>
public class MessagePersistenceHandler : IEventHandler<SendMessageRequestedEvent>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<MessagePersistenceHandler> _logger;
    
    // Batch buffer
    private static readonly ConcurrentQueue<SendMessageRequestedEvent> _messageBuffer = new();
    private static readonly SemaphoreSlim _flushLock = new(1, 1);
    private static DateTime _lastFlushTime = DateTime.UtcNow;
    
    // Configuration (sẽ move to appsettings)
    private const int BATCH_SIZE = 50; // Số messages tối đa trong 1 batch
    private const int FLUSH_INTERVAL_MS = 1000; // Flush mỗi 1 giây

    public MessagePersistenceHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IEventBus eventBus,
        ILogger<MessagePersistenceHandler> logger)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task HandleAsync(SendMessageRequestedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "📥 Received SendMessageRequestedEvent: TempId={TempId}, ConversationId={ConversationId}",
                @event.TemporaryMessageId,
                @event.ConversationId);

            // Add to buffer
            _messageBuffer.Enqueue(@event);
            
            // Check if should flush (batch size OR time interval)
            var shouldFlush = _messageBuffer.Count >= BATCH_SIZE ||
                              (DateTime.UtcNow - _lastFlushTime).TotalMilliseconds >= FLUSH_INTERVAL_MS;
            
            if (shouldFlush)
            {
                await FlushBatchAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error processing SendMessageRequestedEvent: TempId={TempId}",
                @event.TemporaryMessageId);
            throw; // Kafka sẽ retry
        }
    }

    private async Task FlushBatchAsync(CancellationToken cancellationToken)
    {
        // Ensure only one thread flushes at a time
        if (!await _flushLock.WaitAsync(0, cancellationToken))
        {
            return; // Another thread is already flushing
        }

        try
        {
            var batch = new List<SendMessageRequestedEvent>();
            
            // Dequeue up to BATCH_SIZE messages
            while (batch.Count < BATCH_SIZE && _messageBuffer.TryDequeue(out var evt))
            {
                batch.Add(evt);
            }

            if (batch.Count == 0)
            {
                return; // No messages to flush
            }

            _logger.LogInformation(
                "🔥 Flushing batch: {Count} messages to MongoDB",
                batch.Count);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Process batch
            foreach (var evt in batch)
            {
                await ProcessSingleMessageAsync(evt, cancellationToken);
            }

            stopwatch.Stop();
            
            _logger.LogInformation(
                "✅ Batch flush completed: {Count} messages in {ElapsedMs}ms (avg: {AvgMs}ms/message)",
                batch.Count,
                stopwatch.ElapsedMilliseconds,
                stopwatch.ElapsedMilliseconds / batch.Count);

            _lastFlushTime = DateTime.UtcNow;
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private async Task ProcessSingleMessageAsync(SendMessageRequestedEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            // Get conversation to fetch sender info
            var conversation = await _conversationRepository.GetByIdAsync(evt.ConversationId, cancellationToken);
            if (conversation == null)
            {
                _logger.LogWarning(
                    "⚠️ Conversation not found: {ConversationId} for TempId={TempId}",
                    evt.ConversationId,
                    evt.TemporaryMessageId);
                return;
            }

            var sender = conversation.Members.FirstOrDefault(m => m.UserId == evt.SenderId);
            if (sender == null)
            {
                _logger.LogWarning(
                    "⚠️ Sender not in conversation: SenderId={SenderId}, ConversationId={ConversationId}",
                    evt.SenderId,
                    evt.ConversationId);
                return;
            }

            // Get reply message content if exists
            string? replyToContent = null;
            if (!string.IsNullOrEmpty(evt.ReplyToMessageId))
            {
                var replyMessage = await _messageRepository.GetByIdAsync(evt.ReplyToMessageId, cancellationToken);
                replyToContent = replyMessage?.Content;
            }

            // Create message entity
            var message = new ChatMessage
            {
                ConversationId = evt.ConversationId,
                SenderId = evt.SenderId,
                SenderName = sender.UserName, // Get from conversation member
                SenderType = (Message.Domain.Enums.UserType)evt.SenderType,
                Content = evt.Content,
                Status = MessageStatus.Sent,
                ReplyToMessageId = evt.ReplyToMessageId,
                ReplyToContent = replyToContent,
                Attachments = evt.Attachments.Select(a => new MessageAttachment
                {
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    FileType = a.FileType,
                    FileSize = a.FileSize
                }).ToList(),
                SentAt = evt.RequestedAt
            };

            // 💾 SAVE TO MONGODB
            var saved = await _messageRepository.CreateAsync(message, cancellationToken);

            // Update conversation metadata
            await _conversationRepository.UpdateLastMessageAsync(
                evt.ConversationId,
                new LastMessage
                {
                    Content = evt.Content,
                    SenderId = evt.SenderId,
                    SenderName = sender.UserName,
                    SentAt = saved.SentAt,
                    HasAttachment = evt.Attachments.Any()
                },
                cancellationToken);

            await _conversationRepository.IncrementMessageCountAsync(evt.ConversationId, cancellationToken);

            // Update unread counts
            foreach (var member in conversation.Members.Where(m => m.UserId != evt.SenderId))
            {
                await _conversationRepository.UpdateUnreadCountAsync(
                    evt.ConversationId,
                    member.UserId,
                    1,
                    cancellationToken);
            }

            _logger.LogInformation(
                "💾 Message persisted: TempId={TempId} → MongoId={MongoId}",
                evt.TemporaryMessageId,
                saved.Id);

            // 📤 PUBLISH MessageSentEvent for SignalR broadcast
            var messageSentEvent = new MessageSentEvent
            {
                MessageId = saved.Id, // Real MongoDB ID
                ConversationId = saved.ConversationId,
                SenderId = saved.SenderId,
                SenderName = saved.SenderName,
                Content = saved.Content,
                HasAttachment = saved.Attachments.Any(),
                AttachmentCount = saved.Attachments.Count,
                ReplyToMessageId = saved.ReplyToMessageId,
                SentAt = saved.SentAt,
                MessageData = new MessageDto
                {
                    Id = saved.Id,
                    ConversationId = saved.ConversationId,
                    SenderId = saved.SenderId,
                    SenderName = saved.SenderName,
                    SenderType = saved.SenderType,
                    Content = saved.Content,
                    Status = saved.Status,
                    Attachments = saved.Attachments.Select(a => new MessageAttachmentDto
                    {
                        FileName = a.FileName,
                        FileUrl = a.FileUrl,
                        FileType = a.FileType,
                        FileSize = a.FileSize
                    }).ToList(),
                    ReplyToMessageId = saved.ReplyToMessageId,
                    ReplyToContent = saved.ReplyToContent,
                    ReadBy = new List<MessageReadStatusDto>(),
                    SentAt = saved.SentAt,
                    IsDeleted = false
                }
            };

            await _eventBus.PublishAsync(messageSentEvent, cancellationToken);

            _logger.LogInformation(
                "📤 Published MessageSentEvent: MessageId={MessageId}",
                saved.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to persist message: TempId={TempId}, ConversationId={ConversationId}",
                evt.TemporaryMessageId,
                evt.ConversationId);
            throw; // Kafka will retry
        }
    }
}
