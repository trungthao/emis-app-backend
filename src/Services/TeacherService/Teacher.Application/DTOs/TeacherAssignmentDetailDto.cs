namespace Teacher.Application.DTOs
{
    /// <summary>
    /// Assignment kèm thông tin chi tiết lớp học
    /// </summary>
    public class TeacherAssignmentDetailDto
    {
        public Guid AssignmentId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid ClassId { get; set; }
        public string? Role { get; set; }
        public DateTime AssignedAt { get; set; }
        
        // Thông tin chi tiết lớp học
        public string ClassName { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public string? AcademicYear { get; set; }
        public int? TotalStudents { get; set; }
    }
}
