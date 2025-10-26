using Message.Domain.Entities;

namespace Message.Domain.Repositories;

/// <summary>
/// Repository cho Conversation
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Lấy conversation theo ID
    /// </summary>
    Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách conversations của user
    /// </summary>
    Task<List<Conversation>> GetByUserIdAsync(string userId, int skip = 0, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm conversation 1-1 giữa 2 users
    /// </summary>
    Task<Conversation?> FindDirectConversationAsync(string userId1, string userId2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm nhóm chat của học sinh
    /// </summary>
    Task<Conversation?> FindStudentGroupAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo conversation mới
    /// </summary>
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật conversation
    /// </summary>
    Task<bool> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa conversation (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(string id, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật tin nhắn cuối cùng
    /// </summary>
    Task<bool> UpdateLastMessageAsync(string conversationId, LastMessage lastMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tăng số lượng tin nhắn
    /// </summary>
    Task<bool> IncrementMessageCountAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật số tin nhắn chưa đọc cho member
    /// </summary>
    Task<bool> UpdateUnreadCountAsync(string conversationId, string userId, int increment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset số tin nhắn chưa đọc
    /// </summary>
    Task<bool> ResetUnreadCountAsync(string conversationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm thành viên vào conversation
    /// </summary>
    Task<bool> AddMemberAsync(string conversationId, ConversationMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa thành viên khỏi conversation
    /// </summary>
    Task<bool> RemoveMemberAsync(string conversationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật last seen của member
    /// </summary>
    Task<bool> UpdateLastSeenAsync(string conversationId, string userId, CancellationToken cancellationToken = default);
}
