namespace Student.Application.DTOs;

/// <summary>
/// DTO for Grade information
/// </summary>
public class GradeDto
{
    public Guid Id { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public string? GradeCode { get; set; }
    public int Level { get; set; }
    public int? AgeFrom { get; set; }
    public int? AgeTo { get; set; }
    public string? AgeRange { get; set; } // For display: "18-36 tháng" or "3-4 tuổi"
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
