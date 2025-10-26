namespace Teacher.Application.DTOs
{
    /// <summary>
    /// Thông tin cơ bản của lớp học
    /// </summary>
    public class ClassInfoDto
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public string? AcademicYear { get; set; }
        public int? TotalStudents { get; set; }
    }
}
