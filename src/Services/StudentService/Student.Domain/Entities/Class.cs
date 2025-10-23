using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity đại diện cho lớp học
/// </summary>
public class Class : BaseEntity
{
    public string ClassName { get; private set; } = string.Empty;
    public string? ClassCode { get; private set; }
    public Guid GradeId { get; private set; }
    public Grade Grade { get; private set; } = null!;
    public int Capacity { get; private set; }
    public int CurrentStudentCount { get; private set; }
    public string? Room { get; private set; }
    public ClassStatus Status { get; private set; }
    
    // Teacher assignment
    public Guid? HeadTeacherId { get; private set; }
    public string? HeadTeacherName { get; private set; } // Denormalized for performance
    
    // Navigation properties
    private readonly List<StudentEntity> _students = new();
    public IReadOnlyCollection<StudentEntity> Students => _students.AsReadOnly();
    
    private readonly List<ClassStudent> _classStudents = new();
    public IReadOnlyCollection<ClassStudent> ClassStudents => _classStudents.AsReadOnly();

    private Class() { } // For EF Core

    public Class(
        string className,
        string? classCode,
        Guid gradeId,
        int capacity)
    {
        ClassName = className;
        ClassCode = classCode;
        GradeId = gradeId;
        Capacity = capacity;
        CurrentStudentCount = 0;
        Status = ClassStatus.Active;
    }

    public void UpdateBasicInfo(
        string className,
        string? classCode,
        int capacity,
        string? room)
    {
        ClassName = className;
        ClassCode = classCode;
        Capacity = capacity;
        Room = room;
    }

    public void AssignHeadTeacher(Guid teacherId, string teacherName)
    {
        HeadTeacherId = teacherId;
        HeadTeacherName = teacherName;
    }

    public void RemoveHeadTeacher()
    {
        HeadTeacherId = null;
        HeadTeacherName = null;
    }

    public bool CanAddStudent()
    {
        return CurrentStudentCount < Capacity && Status == ClassStatus.Active;
    }

    public void IncrementStudentCount()
    {
        if (!CanAddStudent())
        {
            throw new InvalidOperationException("Cannot add more students. Class is full or inactive.");
        }
        CurrentStudentCount++;
    }

    public void DecrementStudentCount()
    {
        if (CurrentStudentCount > 0)
        {
            CurrentStudentCount--;
        }
    }

    public void UpdateStudentCount(int count)
    {
        if (count < 0)
        {
            throw new ArgumentException("Student count cannot be negative", nameof(count));
        }
        if (count > Capacity)
        {
            throw new InvalidOperationException($"Student count ({count}) cannot exceed capacity ({Capacity})");
        }
        CurrentStudentCount = count;
    }

    public void ChangeStatus(ClassStatus newStatus)
    {
        Status = newStatus;
    }

    public void AddStudent(ClassStudent classStudent)
    {
        _classStudents.Add(classStudent);
        IncrementStudentCount();
    }

    public void RemoveStudent(Guid studentId)
    {
        var classStudent = _classStudents.FirstOrDefault(cs => cs.StudentId == studentId);
        if (classStudent != null)
        {
            _classStudents.Remove(classStudent);
            DecrementStudentCount();
        }
    }
}

public enum ClassStatus
{
    Active = 1,
    Inactive = 2,
    Completed = 3
}
