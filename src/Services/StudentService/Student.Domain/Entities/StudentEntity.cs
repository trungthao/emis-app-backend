using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity đại diện cho học sinh trong hệ thống
/// </summary>
public class StudentEntity : BaseEntity
{
    public string StudentCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{LastName} {FirstName}";
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public StudentStatus Status { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    
    // Health information
    public string? BloodType { get; private set; }
    public string? Allergies { get; private set; }
    public string? MedicalNotes { get; private set; }
    
    // Academic information
    public Guid? CurrentClassId { get; private set; }
    public Class? CurrentClass { get; private set; }
    
    // Navigation properties
    private readonly List<ParentStudent> _parents = new();
    public IReadOnlyCollection<ParentStudent> Parents => _parents.AsReadOnly();
    
    private readonly List<ClassStudent> _classHistory = new();
    public IReadOnlyCollection<ClassStudent> ClassHistory => _classHistory.AsReadOnly();

    private StudentEntity() { } // For EF Core

    public StudentEntity(
        string studentCode,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        DateTime enrollmentDate)
    {
        StudentCode = studentCode;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        EnrollmentDate = enrollmentDate;
        Status = StudentStatus.Active;
    }

    public void UpdateBasicInfo(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    public void UpdateContactInfo(
        string? address,
        string? phone,
        string? email)
    {
        Address = address;
        Phone = phone;
        Email = email;
    }

    public void UpdateHealthInfo(
        string? bloodType,
        string? allergies,
        string? medicalNotes)
    {
        BloodType = bloodType;
        Allergies = allergies;
        MedicalNotes = medicalNotes;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void AssignToClass(Guid classId)
    {
        CurrentClassId = classId;
    }

    public void RemoveFromClass()
    {
        CurrentClassId = null;
    }

    public void ChangeStatus(StudentStatus newStatus)
    {
        Status = newStatus;
    }

    public void AddParent(ParentStudent parentStudent)
    {
        _parents.Add(parentStudent);
    }

    public void RemoveParent(Guid parentId)
    {
        var parent = _parents.FirstOrDefault(p => p.ParentId == parentId);
        if (parent != null)
        {
            _parents.Remove(parent);
        }
    }
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum StudentStatus
{
    Active = 1,
    Inactive = 2,
    Graduated = 3,
    Transferred = 4,
    Suspended = 5
}
