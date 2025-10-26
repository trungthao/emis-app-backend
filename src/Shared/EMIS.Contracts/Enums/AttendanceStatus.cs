namespace EMIS.Contracts.Enums;

/// <summary>
/// Daily attendance status
/// </summary>
public enum AttendanceStatus
{
    /// <summary>
    /// Student is present
    /// </summary>
    Present = 1,

    /// <summary>
    /// Student is absent without excuse
    /// </summary>
    Absent = 2,

    /// <summary>
    /// Student arrived late
    /// </summary>
    Late = 3,

    /// <summary>
    /// Student is absent with valid excuse
    /// </summary>
    Excused = 4,

    /// <summary>
    /// Student left early
    /// </summary>
    LeftEarly = 5,

    /// <summary>
    /// Not yet marked
    /// </summary>
    NotMarked = 0
}
