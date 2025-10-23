using MediatR;
using Student.Application.Common;
using Student.Domain.Repositories;

namespace Student.Application.UseCases.Parents.Commands.RemoveParentFromStudent;

public class RemoveParentFromStudentCommandHandler : IRequestHandler<RemoveParentFromStudentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveParentFromStudentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveParentFromStudentCommand request, CancellationToken cancellationToken)
    {
        // Tìm parent
        var parent = await _unitOfWork.Parents.GetByIdAsync(request.ParentId, cancellationToken);
        if (parent == null)
        {
            return Result<bool>.Failure("Không tìm thấy phụ huynh");
        }

        // Xóa relationship
        parent.RemoveStudent(request.StudentId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
