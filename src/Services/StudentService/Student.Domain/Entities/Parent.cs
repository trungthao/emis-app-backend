using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity đại diện cho phụ huynh của học sinh
/// </summary>
public class Parent : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{LastName} {FirstName}";
    public DateTime? DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Occupation { get; private set; }
    public string? WorkPlace { get; private set; }
    public string? AvatarUrl { get; private set; }
    
    // Authentication
    public string? UserId { get; private set; } // Link to Identity user
    
    // Navigation properties
    private readonly List<ParentStudent> _students = new();
    public IReadOnlyCollection<ParentStudent> Students => _students.AsReadOnly();

    private Parent() { } // For EF Core

    public Parent(
        string firstName,
        string lastName,
        Gender gender,
        string phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        Phone = phone;
    }

    public void UpdateBasicInfo(
        string firstName,
        string lastName,
        Gender gender,
        DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
    }

    public void UpdateContactInfo(
        string phone,
        string? email,
        string? address)
    {
        Phone = phone;
        Email = email;
        Address = address;
    }

    public void UpdateWorkInfo(
        string? occupation,
        string? workPlace)
    {
        Occupation = occupation;
        WorkPlace = workPlace;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void LinkToUser(string userId)
    {
        UserId = userId;
    }

    public void AddStudent(ParentStudent parentStudent)
    {
        _students.Add(parentStudent);
    }

    public void RemoveStudent(Guid studentId)
    {
        var student = _students.FirstOrDefault(s => s.StudentId == studentId);
        if (student != null)
        {
            _students.Remove(student);
        }
    }
}
