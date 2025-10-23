using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Students.Queries.GetStudentById;

/// <summary>
/// Handler để xử lý GetStudentByIdQuery
/// </summary>
public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStudentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId, cancellationToken);

        if (student == null)
        {
            return Result<StudentDto>.Failure($"Không tìm thấy học sinh với Id: {request.StudentId}");
        }

        var studentDto = new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FirstName = student.FirstName,
            LastName = student.LastName,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender.ToString(),
            PlaceOfBirth = student.PlaceOfBirth,
            Address = student.Address,
            Phone = student.Phone,
            Email = student.Email,
            AvatarUrl = student.AvatarUrl,
            Status = student.Status.ToString(),
            EnrollmentDate = student.EnrollmentDate,
            BloodType = student.BloodType,
            Allergies = student.Allergies,
            MedicalNotes = student.MedicalNotes,
            CurrentClassId = student.CurrentClassId,
            CurrentClassName = student.CurrentClass?.ClassName,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };

        return Result<StudentDto>.Success(studentDto);
    }
}
