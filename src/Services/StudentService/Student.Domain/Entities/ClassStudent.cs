using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity quan hệ giữa Class và Student, lưu lịch sử học của học sinh
/// </summary>
public class ClassStudent : BaseEntity
{
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    
    public Guid StudentId { get; private set; }
    public StudentEntity Student { get; private set; } = null!;
    
    public DateTime JoinDate { get; private set; }
    public DateTime? LeaveDate { get; private set; }
    public ClassStudentStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private ClassStudent() { } // For EF Core

    public ClassStudent(
        Guid classId,
        Guid studentId,
        DateTime joinDate)
    {
        ClassId = classId;
        StudentId = studentId;
        JoinDate = joinDate;
        Status = ClassStudentStatus.Active;
    }

    public void LeaveClass(DateTime leaveDate, string? notes = null)
    {
        LeaveDate = leaveDate;
        Status = ClassStudentStatus.Left;
        Notes = notes;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}

public enum ClassStudentStatus
{
    Active = 1,
    Left = 2,
    Transferred = 3
}
