namespace Message.Domain.Enums;

/// <summary>
/// Vai trò thành viên trong nhóm chat
/// </summary>
public enum MemberRole
{
    /// <summary>
    /// Thành viên thông thường
    /// </summary>
    Member = 1,

    /// <summary>
    /// Quản trị viên nhóm (có thể thêm/xóa thành viên, đổi tên nhóm)
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Người tạo nhóm
    /// </summary>
    Owner = 3
}
