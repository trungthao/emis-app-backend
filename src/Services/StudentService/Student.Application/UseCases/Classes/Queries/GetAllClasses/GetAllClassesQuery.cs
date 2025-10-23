using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;

namespace Student.Application.UseCases.Classes.Queries.GetAllClasses;

/// <summary>
/// Query để lấy danh sách tất cả lớp học
/// </summary>
public record GetAllClassesQuery : IRequest<Result<IEnumerable<ClassDto>>>
{
    public Guid? GradeId { get; init; }
    public bool? OnlyActive { get; init; } = true;
}
