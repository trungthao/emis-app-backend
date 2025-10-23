using MediatR;
using Student.Application.Common;
using Student.Domain.Entities;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Parents.Commands.AddParentToStudent;

public class AddParentToStudentCommandHandler : IRequestHandler<AddParentToStudentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddParentToStudentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddParentToStudentCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra học sinh tồn tại
        var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<Guid>.Failure("Không tìm thấy học sinh");
        }

        // Kiểm tra xem phụ huynh đã tồn tại chưa (dựa vào phone hoặc email)
        Parent? parent = null;
        
        if (!string.IsNullOrEmpty(request.Phone))
        {
            parent = await _unitOfWork.Parents.GetByPhoneAsync(request.Phone, cancellationToken);
        }
        
        if (parent == null && !string.IsNullOrEmpty(request.Email))
        {
            parent = await _unitOfWork.Parents.GetByEmailAsync(request.Email, cancellationToken);
        }

        if (parent == null)
        {
            // Tạo phụ huynh mới
            parent = new Parent(
                request.FirstName,
                request.LastName,
                (Gender)request.Gender,
                request.Phone
            );
            
            if (request.DateOfBirth.HasValue)
            {
                parent.UpdateBasicInfo(
                    request.FirstName,
                    request.LastName,
                    (Gender)request.Gender,
                    request.DateOfBirth
                );
            }
            
            parent.UpdateContactInfo(request.Phone, request.Email, request.Address);
            parent.UpdateWorkInfo(request.Occupation, request.WorkPlace);
            
            await _unitOfWork.Parents.AddAsync(parent, cancellationToken);
        }

        // Tạo quan hệ Parent-Student
        var parentStudent = new ParentStudent(
            parent.Id,
            student.Id,
            (RelationshipType)request.RelationshipType,
            request.IsPrimaryContact
        );
        
        parentStudent.SetPickUpPermission(request.CanPickUp);
        parentStudent.SetNotificationPreference(request.ReceiveNotifications);
        
        parent.AddStudent(parentStudent);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(parent.Id);
    }
}
