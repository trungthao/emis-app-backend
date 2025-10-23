using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity đại diện cho khối lớp (Grade Level)
/// Ví dụ: Lớp 1, Lớp 2, Lớp 3...
/// </summary>
public class Grade : BaseEntity
{
    public string GradeName { get; private set; } = string.Empty;
    public string? GradeCode { get; private set; }
    public int Level { get; private set; } // 1, 2, 3, 4, 5...
    public int? AgeFrom { get; private set; } // Độ tuổi từ
    public int? AgeTo { get; private set; } // Độ tuổi đến
    public string? Description { get; private set; }
    public GradeStatus Status { get; private set; }
    
    // Navigation properties
    private readonly List<Class> _classes = new();
    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();

    private Grade() { } // For EF Core

    public Grade(
        string gradeName,
        string? gradeCode,
        int level,
        int? ageFrom = null,
        int? ageTo = null)
    {
        GradeName = gradeName;
        GradeCode = gradeCode;
        Level = level;
        AgeFrom = ageFrom;
        AgeTo = ageTo;
        Status = GradeStatus.Active;
    }

    public void UpdateInfo(
        string gradeName,
        string? gradeCode,
        int level,
        int? ageFrom,
        int? ageTo,
        string? description)
    {
        GradeName = gradeName;
        GradeCode = gradeCode;
        Level = level;
        AgeFrom = ageFrom;
        AgeTo = ageTo;
        Description = description;
    }

    public void ChangeStatus(GradeStatus newStatus)
    {
        Status = newStatus;
    }
}

public enum GradeStatus
{
    Active = 1,
    Inactive = 2
}
