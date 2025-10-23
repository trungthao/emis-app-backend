using MediatR;
using Student.Application.Common;

namespace Student.Application.UseCases.Students.Commands.AssignStudentToClass;

/// <summary>
/// Command để phân lớp cho học sinh
/// </summary>
public record AssignStudentToClassCommand : IRequest<Result<bool>>
{
    public Guid StudentId { get; init; }
    public Guid ClassId { get; init; }
    public DateTime? JoinDate { get; init; }
    public string? Notes { get; init; }
}
