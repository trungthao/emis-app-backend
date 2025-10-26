namespace Message.Domain.Enums;

/// <summary>
/// Loại người dùng trong hệ thống
/// </summary>
public enum UserType
{
    /// <summary>
    /// Giáo viên
    /// </summary>
    Teacher = 1,
    
    /// <summary>
    /// Phụ huynh
    /// </summary>
    Parent = 2,
    
    /// <summary>
    /// Quản trị viên
    /// </summary>
    Admin = 3
}
