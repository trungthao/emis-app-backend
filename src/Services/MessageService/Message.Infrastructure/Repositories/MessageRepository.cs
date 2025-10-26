using MongoDB.Driver;
using Message.Domain.Entities;
using Message.Domain.Repositories;
using Message.Infrastructure.Persistence;

namespace Message.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly MessageDbContext _context;

    public MessageRepository(MessageDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Find(m => m.Id == id && !m.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> GetByConversationIdAsync(
        string conversationId,
        int skip = 0,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Find(m => m.ConversationId == conversationId && !m.IsDeleted)
            .SortByDescending(m => m.SentAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> GetMessagesAfterAsync(
        string conversationId,
        DateTime after,
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Find(m => m.ConversationId == conversationId && 
                      m.SentAt > after && 
                      !m.IsDeleted)
            .SortBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> SearchMessagesAsync(
        string conversationId,
        string searchText,
        int skip = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ChatMessage>.Filter.And(
            Builders<ChatMessage>.Filter.Eq(m => m.ConversationId, conversationId),
            Builders<ChatMessage>.Filter.Text(searchText),
            Builders<ChatMessage>.Filter.Eq(m => m.IsDeleted, false)
        );

        return await _context.Messages
            .Find(filter)
            .SortByDescending(m => m.SentAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatMessage> CreateAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        await _context.Messages.InsertOneAsync(message, cancellationToken: cancellationToken);
        return message;
    }

    public async Task<bool> UpdateAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        message.EditedAt = DateTime.UtcNow;
        var result = await _context.Messages
            .ReplaceOneAsync(m => m.Id == message.Id, message, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<ChatMessage>.Update
            .Set(m => m.IsDeleted, true)
            .Set(m => m.DeletedAt, DateTime.UtcNow);

        var result = await _context.Messages
            .UpdateOneAsync(m => m.Id == id, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkAsReadAsync(
        string messageId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ChatMessage>.Filter.And(
            Builders<ChatMessage>.Filter.Eq(m => m.Id, messageId),
            Builders<ChatMessage>.Filter.Not(
                Builders<ChatMessage>.Filter.ElemMatch(m => m.ReadBy, r => r.UserId == userId)
            )
        );

        var update = Builders<ChatMessage>.Update
            .Push(m => m.ReadBy, new MessageReadStatus
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

        var result = await _context.Messages
            .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(
        string conversationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ChatMessage>.Filter.And(
            Builders<ChatMessage>.Filter.Eq(m => m.ConversationId, conversationId),
            Builders<ChatMessage>.Filter.Ne(m => m.SenderId, userId),
            Builders<ChatMessage>.Filter.Not(
                Builders<ChatMessage>.Filter.ElemMatch(m => m.ReadBy, r => r.UserId == userId)
            )
        );

        var update = Builders<ChatMessage>.Update
            .Push(m => m.ReadBy, new MessageReadStatus
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

        var result = await _context.Messages
            .UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<int> CountUnreadMessagesAsync(
        string conversationId,
        string userId,
        DateTime? lastSeenAt,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<ChatMessage>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Eq(m => m.ConversationId, conversationId),
            filterBuilder.Ne(m => m.SenderId, userId),
            filterBuilder.Not(
                filterBuilder.ElemMatch(m => m.ReadBy, r => r.UserId == userId)
            )
        );

        if (lastSeenAt.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Gt(m => m.SentAt, lastSeenAt.Value));
        }

        return (int)await _context.Messages.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
