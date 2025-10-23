using MediatR;
using Student.Application.Common;
using Student.Domain.Entities;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Parents.Commands.UpdateParentRelationship;

public class UpdateParentRelationshipCommandHandler : IRequestHandler<UpdateParentRelationshipCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateParentRelationshipCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdateParentRelationshipCommand request, CancellationToken cancellationToken)
    {
        // Tìm parent
        var parent = await _unitOfWork.Parents.GetByIdAsync(request.ParentId, cancellationToken);
        if (parent == null)
        {
            return Result<bool>.Failure("Không tìm thấy phụ huynh");
        }

        // Tìm relationship
        var parentStudent = parent.Students.FirstOrDefault(ps => ps.StudentId == request.StudentId);
        if (parentStudent == null)
        {
            return Result<bool>.Failure("Không tìm thấy mối quan hệ giữa phụ huynh và học sinh");
        }

        // Cập nhật relationship
        parentStudent.UpdateRelationship((RelationshipType)request.RelationshipType);
        parentStudent.SetAsPrimaryContact(request.IsPrimaryContact);
        parentStudent.SetPickUpPermission(request.CanPickUp);
        parentStudent.SetNotificationPreference(request.ReceiveNotifications);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
