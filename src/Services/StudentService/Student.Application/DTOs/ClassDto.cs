namespace Student.Application.DTOs;

/// <summary>
/// DTO for Class information
/// </summary>
public class ClassDto
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? ClassCode { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public int Capacity { get; set; }
    public int CurrentStudentCount { get; set; }
    public int AvailableSeats { get; set; }
    public string? Room { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? HeadTeacherId { get; set; }
    public string? HeadTeacherName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
