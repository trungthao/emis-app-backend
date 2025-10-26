namespace Message.Domain.Enums;

/// <summary>
/// Trạng thái tin nhắn
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// Đã gửi
    /// </summary>
    Sent = 1,
    
    /// <summary>
    /// Đã nhận (đã đến server)
    /// </summary>
    Delivered = 2,
    
    /// <summary>
    /// Đã đọc
    /// </summary>
    Read = 3,
    
    /// <summary>
    /// Đã xóa
    /// </summary>
    Deleted = 4
}
