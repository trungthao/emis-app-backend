using MediatR;
using Student.Application.Common;

namespace Student.Application.UseCases.Parents.Commands.RemoveParentFromStudent;

/// <summary>
/// Command để xóa liên kết phụ huynh - học sinh
/// </summary>
public record RemoveParentFromStudentCommand : IRequest<Result<bool>>
{
    public Guid StudentId { get; init; }
    public Guid ParentId { get; init; }
}
