using Message.Domain.Entities;

namespace Message.Domain.Repositories;

/// <summary>
/// Repository cho ChatMessage
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Lấy tin nhắn theo ID
    /// </summary>
    Task<ChatMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy danh sách tin nhắn của conversation
    /// </summary>
    Task<List<ChatMessage>> GetByConversationIdAsync(
        string conversationId, 
        int skip = 0, 
        int limit = 50, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy tin nhắn mới hơn một timestamp
    /// </summary>
    Task<List<ChatMessage>> GetMessagesAfterAsync(
        string conversationId, 
        DateTime after, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tìm kiếm tin nhắn
    /// </summary>
    Task<List<ChatMessage>> SearchMessagesAsync(
        string conversationId, 
        string searchText, 
        int skip = 0, 
        int limit = 20, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tạo tin nhắn mới
    /// </summary>
    Task<ChatMessage> CreateAsync(ChatMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cập nhật tin nhắn
    /// </summary>
    Task<bool> UpdateAsync(ChatMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Xóa tin nhắn (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Đánh dấu tin nhắn đã đọc
    /// </summary>
    Task<bool> MarkAsReadAsync(string messageId, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Đánh dấu tất cả tin nhắn trong conversation là đã đọc
    /// </summary>
    Task<bool> MarkAllAsReadAsync(string conversationId, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Đếm số tin nhắn chưa đọc trong conversation
    /// </summary>
    Task<int> CountUnreadMessagesAsync(string conversationId, string userId, DateTime? lastSeenAt, CancellationToken cancellationToken = default);
}
