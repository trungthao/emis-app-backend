namespace EMIS.Contracts.Events;

/// <summary>
/// Event raised when a new student is created
/// This event can be consumed by multiple services (AuthService, NotificationService, etc.)
/// </summary>
public class StudentCreatedEvent : BaseEvent
{
    public override string EventType => "emis.student.created";

    /// <summary>
    /// Student's unique identifier
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Full name of the student
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address (will be used as username, may be optional for young students)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number (may be optional for young students)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Grade level
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>
    /// Class name
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Default password (should be hashed before storing)
    /// </summary>
    public string DefaultPassword { get; set; } = string.Empty;

    /// <summary>
    /// School ID
    /// </summary>
    public Guid? SchoolId { get; set; }
}
