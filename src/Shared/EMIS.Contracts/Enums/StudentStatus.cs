namespace EMIS.Contracts.Enums;

/// <summary>
/// Student enrollment status
/// </summary>
public enum StudentStatus
{
    /// <summary>
    /// Currently enrolled and active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Temporarily inactive (leave of absence)
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Transferred to another school
    /// </summary>
    Transferred = 3,

    /// <summary>
    /// Graduated from the school
    /// </summary>
    Graduated = 4,

    /// <summary>
    /// Dropped out
    /// </summary>
    Dropped = 5,

    /// <summary>
    /// Suspended
    /// </summary>
    Suspended = 6,

    /// <summary>
    /// Expelled
    /// </summary>
    Expelled = 7,

    /// <summary>
    /// Pending enrollment approval
    /// </summary>
    Pending = 8
}
