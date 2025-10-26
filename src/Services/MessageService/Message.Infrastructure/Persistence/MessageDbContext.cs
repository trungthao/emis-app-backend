using Message.Domain.Entities;
using Message.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Message.Infrastructure.Persistence;

/// <summary>
/// MongoDB Context
/// </summary>
public class MessageDbContext
{
    private readonly IMongoDatabase _database;

    public MessageDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        // Create indexes
        CreateIndexes();
    }

    public IMongoCollection<Conversation> Conversations =>
        _database.GetCollection<Conversation>(nameof(Conversations).ToLower());

    public IMongoCollection<ChatMessage> Messages =>
        _database.GetCollection<ChatMessage>(nameof(Messages).ToLower());

    private void CreateIndexes()
    {
        // Conversations indexes
        var conversationsIndexes = Conversations.Indexes;

        // Index cho Members.UserId để query conversations của user
        conversationsIndexes.CreateOne(
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys.Ascending(c => c.Members)));

        // Index cho StudentId (Student Group)
        conversationsIndexes.CreateOne(
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys.Ascending(c => c.StudentId)));

        // Index cho Type
        conversationsIndexes.CreateOne(
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys.Ascending(c => c.Type)));

        // Index cho UpdatedAt (để sort theo thời gian)
        conversationsIndexes.CreateOne(
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys.Descending(c => c.UpdatedAt)));

        // Messages indexes
        var messagesIndexes = Messages.Indexes;

        // Index cho ConversationId
        messagesIndexes.CreateOne(
            new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys.Ascending(m => m.ConversationId)));

        // Compound index cho ConversationId + SentAt (để query và sort)
        messagesIndexes.CreateOne(
            new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys
                    .Ascending(m => m.ConversationId)
                    .Descending(m => m.SentAt)));

        // Index cho SenderId
        messagesIndexes.CreateOne(
            new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys.Ascending(m => m.SenderId)));

        // Text index cho Content (để search)
        messagesIndexes.CreateOne(
            new CreateIndexModel<ChatMessage>(
                Builders<ChatMessage>.IndexKeys.Text(m => m.Content)));
    }
}
