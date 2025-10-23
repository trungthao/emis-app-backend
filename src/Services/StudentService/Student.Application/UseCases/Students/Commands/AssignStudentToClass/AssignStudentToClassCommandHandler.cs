using MediatR;
using Student.Application.Common;
using Student.Domain.Entities;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Students.Commands.AssignStudentToClass;

public class AssignStudentToClassCommandHandler : IRequestHandler<AssignStudentToClassCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignStudentToClassCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AssignStudentToClassCommand request, CancellationToken cancellationToken)
    {
        // Tìm học sinh
        var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<bool>.Failure("Không tìm thấy học sinh");
        }

        // Tìm lớp học
        var classEntity = await _unitOfWork.Classes.GetByIdAsync(request.ClassId, cancellationToken);
        if (classEntity == null)
        {
            return Result<bool>.Failure("Không tìm thấy lớp học");
        }

        // Kiểm tra lớp còn chỗ trống không
        if (classEntity.CurrentStudentCount >= classEntity.Capacity)
        {
            return Result<bool>.Failure($"Lớp '{classEntity.ClassName}' đã đầy (Sức chứa: {classEntity.Capacity}/{classEntity.Capacity})");
        }

        // Kiểm tra học sinh đã có lớp chưa
        if (student.CurrentClassId.HasValue)
        {
            if (student.CurrentClassId.Value == request.ClassId)
            {
                return Result<bool>.Failure("Học sinh đã ở trong lớp này rồi");
            }

            // Đánh dấu rời lớp cũ
            // TODO: Mark old ClassStudent as left with LeaveDate
        }

        // Phân lớp
        student.AssignToClass(classEntity.Id);
        
        // Tạo bản ghi ClassStudent
        var joinDate = request.JoinDate ?? DateTime.UtcNow;
        var classStudent = new ClassStudent(classEntity.Id, student.Id, joinDate);
        
        if (!string.IsNullOrEmpty(request.Notes))
        {
            classStudent.UpdateNotes(request.Notes);
        }

        // Cập nhật số lượng học sinh trong lớp
        classEntity.UpdateStudentCount(classEntity.CurrentStudentCount + 1);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
