namespace Message.Domain.Enums;

/// <summary>
/// Loại cuộc hội thoại
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Tin nhắn trực tiếp 1-1 giữa phụ huynh và giáo viên
    /// </summary>
    DirectMessage = 1,

    /// <summary>
    /// Nhóm chat của học sinh (tự động tạo)
    /// Bao gồm: phụ huynh của học sinh + giáo viên phụ trách lớp
    /// </summary>
    StudentGroup = 2,

    /// <summary>
    /// Nhóm chat tùy chỉnh (do người dùng tạo)
    /// </summary>
    CustomGroup = 3
}
