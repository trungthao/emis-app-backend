using MongoDB.Driver;
using Message.Domain.Entities;
using Message.Domain.Repositories;
using Message.Domain.Enums;
using Message.Infrastructure.Persistence;

namespace Message.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly MessageDbContext _context;

    public ConversationRepository(MessageDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Find(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetByUserIdAsync(
        string userId, 
        int skip = 0, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Find(c => c.Members.Any(m => m.UserId == userId && !m.HasLeft) && !c.IsDeleted)
            .SortByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> FindDirectConversationAsync(
        string userId1, 
        string userId2, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Find(c => c.Type == ConversationType.DirectMessage &&
                      c.Members.Count == 2 &&
                      c.Members.Any(m => m.UserId == userId1) &&
                      c.Members.Any(m => m.UserId == userId2) &&
                      !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation?> FindStudentGroupAsync(
        Guid studentId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Find(c => c.Type == ConversationType.StudentGroup &&
                      c.StudentId == studentId &&
                      !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation> CreateAsync(
        Conversation conversation, 
        CancellationToken cancellationToken = default)
    {
        await _context.Conversations.InsertOneAsync(conversation, cancellationToken: cancellationToken);
        return conversation;
    }

    public async Task<bool> UpdateAsync(
        Conversation conversation, 
        CancellationToken cancellationToken = default)
    {
        conversation.UpdatedAt = DateTime.UtcNow;
        var result = await _context.Conversations
            .ReplaceOneAsync(c => c.Id == conversation.Id, conversation, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(
        string id, 
        string deletedBy, 
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Conversation>.Update
            .Set(c => c.IsDeleted, true)
            .Set(c => c.DeletedAt, DateTime.UtcNow)
            .Set(c => c.DeletedBy, deletedBy);

        var result = await _context.Conversations
            .UpdateOneAsync(c => c.Id == id, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateLastMessageAsync(
        string conversationId, 
        LastMessage lastMessage, 
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Conversation>.Update
            .Set(c => c.LastMessage, lastMessage)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(c => c.Id == conversationId, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> IncrementMessageCountAsync(
        string conversationId, 
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Conversation>.Update
            .Inc(c => c.TotalMessages, 1)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(c => c.Id == conversationId, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateUnreadCountAsync(
        string conversationId, 
        string userId, 
        int increment, 
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
            Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId)
        );

        var update = Builders<Conversation>.Update
            .Inc("Members.$.UnreadCount", increment)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ResetUnreadCountAsync(
        string conversationId, 
        string userId, 
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
            Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId)
        );

        var update = Builders<Conversation>.Update
            .Set("Members.$.UnreadCount", 0)
            .Set("Members.$.LastSeenAt", DateTime.UtcNow)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> AddMemberAsync(
        string conversationId, 
        ConversationMember member, 
        CancellationToken cancellationToken = default)
    {
        var update = Builders<Conversation>.Update
            .Push(c => c.Members, member)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(c => c.Id == conversationId, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveMemberAsync(
        string conversationId, 
        string userId, 
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
            Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId)
        );

        var update = Builders<Conversation>.Update
            .Set("Members.$.HasLeft", true)
            .Set("Members.$.LeftAt", DateTime.UtcNow)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateLastSeenAsync(
        string conversationId, 
        string userId, 
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
            Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId)
        );

        var update = Builders<Conversation>.Update
            .Set("Members.$.LastSeenAt", DateTime.UtcNow);

        var result = await _context.Conversations
            .UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        
        return result.ModifiedCount > 0;
    }
}
