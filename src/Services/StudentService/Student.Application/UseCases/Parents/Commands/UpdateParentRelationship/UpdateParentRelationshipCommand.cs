using MediatR;
using Student.Application.Common;

namespace Student.Application.UseCases.Parents.Commands.UpdateParentRelationship;

/// <summary>
/// Command để cập nhật mối quan hệ phụ huynh - học sinh
/// </summary>
public record UpdateParentRelationshipCommand : IRequest<Result<bool>>
{
    public Guid StudentId { get; init; }
    public Guid ParentId { get; init; }
    
    public int RelationshipType { get; init; }
    public bool IsPrimaryContact { get; init; }
    public bool CanPickUp { get; init; }
    public bool ReceiveNotifications { get; init; }
}
