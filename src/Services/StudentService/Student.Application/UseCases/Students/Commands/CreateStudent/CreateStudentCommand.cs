using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;

namespace Student.Application.UseCases.Students.Commands.CreateStudent;

/// <summary>
/// Command để tạo mới học sinh
/// </summary>
public record CreateStudentCommand : IRequest<Result<StudentDto>>
{
    public string StudentCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public DateTime EnrollmentDate { get; init; }
    public string? BloodType { get; init; }
    public string? Allergies { get; init; }
    public string? MedicalNotes { get; init; }
    
    /// <summary>
    /// ID lớp học (tùy chọn)
    /// </summary>
    public Guid? ClassId { get; init; }
    
    /// <summary>
    /// Danh sách phụ huynh (tùy chọn)
    /// </summary>
    public List<CreateParentInfo>? Parents { get; init; }
}

/// <summary>
/// Thông tin phụ huynh khi tạo học sinh
/// </summary>
public record CreateParentInfo
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
    public string? WorkPlace { get; init; }
    
    // Relationship info
    public int RelationshipType { get; init; } // 0=Father, 1=Mother, 2=Guardian, 3=Other
    public bool IsPrimaryContact { get; init; }
    public bool CanPickUp { get; init; } = true;
    public bool ReceiveNotifications { get; init; } = true;
}

