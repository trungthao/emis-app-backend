using Student.Domain.Common;

namespace Student.Domain.Entities;

/// <summary>
/// Entity quan hệ nhiều-nhiều giữa Parent và Student
/// Lưu thông tin về mối quan hệ (cha, mẹ, người giám hộ...)
/// </summary>
public class ParentStudent : BaseEntity
{
    public Guid ParentId { get; private set; }
    public Parent Parent { get; private set; } = null!;
    
    public Guid StudentId { get; private set; }
    public StudentEntity Student { get; private set; } = null!;
    
    public RelationshipType RelationshipType { get; private set; }
    public bool IsPrimaryContact { get; private set; }
    public bool CanPickUp { get; private set; }
    public bool ReceiveNotifications { get; private set; }

    private ParentStudent() { } // For EF Core

    public ParentStudent(
        Guid parentId,
        Guid studentId,
        RelationshipType relationshipType,
        bool isPrimaryContact = false)
    {
        ParentId = parentId;
        StudentId = studentId;
        RelationshipType = relationshipType;
        IsPrimaryContact = isPrimaryContact;
        CanPickUp = true;
        ReceiveNotifications = true;
    }

    public void UpdateRelationship(RelationshipType relationshipType)
    {
        RelationshipType = relationshipType;
    }

    public void SetAsPrimaryContact(bool isPrimary)
    {
        IsPrimaryContact = isPrimary;
    }

    public void SetPickUpPermission(bool canPickUp)
    {
        CanPickUp = canPickUp;
    }

    public void SetNotificationPreference(bool receiveNotifications)
    {
        ReceiveNotifications = receiveNotifications;
    }
}

public enum RelationshipType
{
    Father = 1,
    Mother = 2,
    Guardian = 3,
    Grandparent = 4,
    Other = 5
}
