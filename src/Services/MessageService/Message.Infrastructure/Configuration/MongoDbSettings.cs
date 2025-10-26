namespace Message.Infrastructure.Configuration;

/// <summary>
/// Cấu hình MongoDB
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConversationsCollectionName { get; set; } = "conversations";
    public string MessagesCollectionName { get; set; } = "messages";
}
