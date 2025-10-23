using MediatR;
using Student.Application.Common;

namespace Student.Application.UseCases.Parents.Commands.AddParentToStudent;

/// <summary>
/// Command để thêm phụ huynh cho học sinh đã tồn tại
/// </summary>
public record AddParentToStudentCommand : IRequest<Result<Guid>>
{
    public Guid StudentId { get; init; }
    
    // Parent Info
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
    public string? WorkPlace { get; init; }
    
    // Relationship Info
    public int RelationshipType { get; init; }
    public bool IsPrimaryContact { get; init; }
    public bool CanPickUp { get; init; } = true;
    public bool ReceiveNotifications { get; init; } = true;
}
