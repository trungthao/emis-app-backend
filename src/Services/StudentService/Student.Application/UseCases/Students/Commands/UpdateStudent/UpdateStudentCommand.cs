using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;

namespace Student.Application.UseCases.Students.Commands.UpdateStudent;

/// <summary>
/// Command để cập nhật thông tin học sinh
/// </summary>
public record UpdateStudentCommand : IRequest<Result<StudentDto>>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? BloodType { get; init; }
    public string? Allergies { get; init; }
    public string? MedicalNotes { get; init; }
    
    /// <summary>
    /// ID lớp học mới (null = không đổi, Guid.Empty = xóa lớp)
    /// </summary>
    public Guid? ClassId { get; init; }
}
