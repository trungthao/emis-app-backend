namespace Teacher.Application.DTOs
{
    /// <summary>
    /// Chi tiết đầy đủ của giáo viên kèm thông tin các lớp học phụ trách
    /// </summary>
    public class TeacherDetailDto
    {
        // Thông tin cơ bản giáo viên
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Danh sách lớp học phụ trách kèm thông tin chi tiết
        public List<TeacherAssignmentDetailDto> ClassAssignments { get; set; } = new();
        
        // Thống kê
        public int TotalClassesAssigned { get; set; }
    }
}
