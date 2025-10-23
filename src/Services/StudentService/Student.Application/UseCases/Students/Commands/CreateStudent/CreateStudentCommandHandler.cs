using MediatR;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Domain.Entities;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Students.Commands.CreateStudent;

/// <summary>
/// Handler để xử lý CreateStudentCommand
/// </summary>
public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentDto>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra xem mã học sinh đã tồn tại chưa
        var exists = await _unitOfWork.Students.IsStudentCodeExistsAsync(request.StudentCode, cancellationToken);
        if (exists)
        {
            return Result<StudentDto>.Failure($"Mã học sinh '{request.StudentCode}' đã tồn tại");
        }

        // Tạo entity mới
        var student = new StudentEntity(
            request.StudentCode,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            (Gender)request.Gender,
            request.EnrollmentDate
        );

        // Cập nhật thông tin liên hệ và sức khỏe
        student.UpdateContactInfo(request.Address, request.Phone, request.Email);
        student.UpdateHealthInfo(request.BloodType, request.Allergies, request.MedicalNotes);

        if (!string.IsNullOrEmpty(request.PlaceOfBirth))
        {
            // Sử dụng reflection hoặc thêm method để set PlaceOfBirth
            typeof(StudentEntity)
                .GetProperty("PlaceOfBirth")!
                .SetValue(student, request.PlaceOfBirth);
        }

        // Xử lý phân lớp nếu có
        if (request.ClassId.HasValue)
        {
            var classEntity = await _unitOfWork.Classes.GetByIdAsync(request.ClassId.Value, cancellationToken);
            if (classEntity == null)
            {
                return Result<StudentDto>.Failure("Không tìm thấy lớp học");
            }

            // Kiểm tra lớp còn chỗ trống không
            if (classEntity.CurrentStudentCount >= classEntity.Capacity)
            {
                return Result<StudentDto>.Failure($"Lớp '{classEntity.ClassName}' đã đầy (Sức chứa: {classEntity.Capacity})");
            }

            // Phân lớp cho học sinh
            student.AssignToClass(classEntity.Id);
            
            // Tạo bản ghi ClassStudent
            var classStudent = new ClassStudent(classEntity.Id, student.Id, DateTime.UtcNow);
            // Add to context (will be saved later)
        }

        // Lưu học sinh
        await _unitOfWork.Students.AddAsync(student, cancellationToken);
        
        // Xử lý phụ huynh nếu có
        if (request.Parents != null && request.Parents.Any())
        {
            foreach (var parentInfo in request.Parents)
            {
                // Kiểm tra xem phụ huynh đã tồn tại chưa (dựa vào phone hoặc email)
                Parent? existingParent = null;
                
                if (!string.IsNullOrEmpty(parentInfo.Phone))
                {
                    existingParent = await _unitOfWork.Parents.GetByPhoneAsync(parentInfo.Phone, cancellationToken);
                }
                
                if (existingParent == null && !string.IsNullOrEmpty(parentInfo.Email))
                {
                    existingParent = await _unitOfWork.Parents.GetByEmailAsync(parentInfo.Email, cancellationToken);
                }

                Parent parent;
                
                if (existingParent != null)
                {
                    // Sử dụng phụ huynh đã tồn tại
                    parent = existingParent;
                }
                else
                {
                    // Tạo phụ huynh mới
                    parent = new Parent(
                        parentInfo.FirstName,
                        parentInfo.LastName,
                        (Gender)parentInfo.Gender,
                        parentInfo.Phone
                    );
                    
                    if (parentInfo.DateOfBirth.HasValue)
                    {
                        parent.UpdateBasicInfo(
                            parentInfo.FirstName,
                            parentInfo.LastName,
                            (Gender)parentInfo.Gender,
                            parentInfo.DateOfBirth
                        );
                    }
                    
                    parent.UpdateContactInfo(parentInfo.Phone, parentInfo.Email, parentInfo.Address);
                    parent.UpdateWorkInfo(parentInfo.Occupation, parentInfo.WorkPlace);
                    
                    await _unitOfWork.Parents.AddAsync(parent, cancellationToken);
                }

                // Tạo quan hệ Parent-Student
                var parentStudent = new ParentStudent(
                    parent.Id,
                    student.Id,
                    (RelationshipType)parentInfo.RelationshipType,
                    parentInfo.IsPrimaryContact
                );
                
                // Cập nhật permissions
                parentStudent.SetPickUpPermission(parentInfo.CanPickUp);
                parentStudent.SetNotificationPreference(parentInfo.ReceiveNotifications);
                
                // Add relationship
                parent.AddStudent(parentStudent);
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
