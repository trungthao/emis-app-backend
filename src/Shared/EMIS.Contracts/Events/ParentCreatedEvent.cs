namespace EMIS.Contracts.Events;

/// <summary>
/// Event raised when a new parent is created
/// This event can be consumed by multiple services (AuthService, NotificationService, etc.)
/// </summary>
public class ParentCreatedEvent : BaseEvent
{
    public override string EventType => "emis.parent.created";

    /// <summary>
    /// Parent's unique identifier
    /// </summary>
    public Guid ParentId { get; set; }

    /// <summary>
    /// Full name of the parent
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address (will be used as username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Default password (should be hashed before storing)
    /// </summary>
    public string DefaultPassword { get; set; } = string.Empty;

    /// <summary>
    /// Student IDs that this parent is associated with
    /// </summary>
    public List<Guid> StudentIds { get; set; } = new();
}
