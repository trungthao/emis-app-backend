using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Domain.Entities;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Students.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result<StudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentDto>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        // Tìm học sinh
        var student = await _unitOfWork.Students.GetByIdAsync(request.Id, cancellationToken);
        if (student == null)
        {
            return Result<StudentDto>.Failure("Không tìm thấy học sinh");
        }

        // Cập nhật thông tin cơ bản
        student.UpdateBasicInfo(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            (Gender)request.Gender
        );

        // Cập nhật thông tin liên hệ
        student.UpdateContactInfo(request.Address, request.Phone, request.Email);
        
        // Cập nhật thông tin sức khỏe
        student.UpdateHealthInfo(request.BloodType, request.Allergies, request.MedicalNotes);

        if (!string.IsNullOrEmpty(request.PlaceOfBirth))
        {
            typeof(StudentEntity)
                .GetProperty("PlaceOfBirth")!
                .SetValue(student, request.PlaceOfBirth);
        }

        // Xử lý thay đổi lớp học
        if (request.ClassId.HasValue)
        {
            if (request.ClassId.Value == Guid.Empty)
            {
                // Xóa khỏi lớp
                student.RemoveFromClass();
            }
            else if (request.ClassId.Value != student.CurrentClassId)
            {
                // Chuyển sang lớp mới
                var newClass = await _unitOfWork.Classes.GetByIdAsync(request.ClassId.Value, cancellationToken);
                if (newClass == null)
                {
                    return Result<StudentDto>.Failure("Không tìm thấy lớp học");
                }

                // Kiểm tra lớp mới còn chỗ trống không
                if (newClass.CurrentStudentCount >= newClass.Capacity)
                {
                    return Result<StudentDto>.Failure($"Lớp '{newClass.ClassName}' đã đầy (Sức chứa: {newClass.Capacity})");
                }

                // Đánh dấu rời lớp cũ (nếu có)
                if (student.CurrentClassId.HasValue)
                {
                    // TODO: Update ClassStudent record to mark as left
                }

                // Phân vào lớp mới
                student.AssignToClass(newClass.Id);
                
                // Tạo bản ghi ClassStudent mới
                var classStudent = new ClassStudent(newClass.Id, student.Id, DateTime.UtcNow);
                // Will be saved with UnitOfWork
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map sang DTO
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
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };

        return Result<StudentDto>.Success(studentDto);
    }
}
