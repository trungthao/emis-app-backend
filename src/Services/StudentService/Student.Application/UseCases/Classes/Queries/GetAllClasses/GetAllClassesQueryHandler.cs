using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Classes.Queries.GetAllClasses;

public class GetAllClassesQueryHandler : IRequestHandler<GetAllClassesQuery, Result<IEnumerable<ClassDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllClassesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ClassDto>>> Handle(GetAllClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = await _unitOfWork.Classes.GetAllAsync(cancellationToken);

        // Filter by Grade if specified
        if (request.GradeId.HasValue)
        {
            classes = classes.Where(c => c.GradeId == request.GradeId.Value);
        }

        // Filter active only
        if (request.OnlyActive == true)
        {
            classes = classes.Where(c => c.Status == Domain.Entities.ClassStatus.Active);
        }

        var classDtos = classes.Select(c => new ClassDto
        {
            Id = c.Id,
            ClassName = c.ClassName,
            ClassCode = c.ClassCode,
            GradeId = c.GradeId,
            GradeName = c.Grade.GradeName,
            GradeLevel = c.Grade.Level,
            Capacity = c.Capacity,
            CurrentStudentCount = c.CurrentStudentCount,
            AvailableSeats = c.Capacity - c.CurrentStudentCount,
            Room = c.Room,
            Status = c.Status.ToString(),
            HeadTeacherId = c.HeadTeacherId,
            HeadTeacherName = c.HeadTeacherName,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return Result<IEnumerable<ClassDto>>.Success(classDtos);
    }
}
