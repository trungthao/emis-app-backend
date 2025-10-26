namespace EMIS.Contracts.Events;

/// <summary>
/// Event raised when a new teacher is created
/// This event can be consumed by multiple services (AuthService, NotificationService, etc.)
/// </summary>
public class TeacherCreatedEvent : BaseEvent
{
    public override string EventType => "emis.teacher.created";

    /// <summary>
    /// Teacher's unique identifier
    /// </summary>
    public Guid TeacherId { get; set; }

    /// <summary>
    /// Full name of the teacher
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address (will be used as username)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Subject taught by the teacher
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Default password (should be hashed before storing)
    /// </summary>
    public string DefaultPassword { get; set; } = string.Empty;

    /// <summary>
    /// School or organization ID
    /// </summary>
    public Guid? SchoolId { get; set; }
}
