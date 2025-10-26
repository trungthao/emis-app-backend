namespace EMIS.Contracts.Enums;

/// <summary>
/// Notification delivery channels
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Email notification
    /// </summary>
    Email = 1,

    /// <summary>
    /// SMS notification
    /// </summary>
    SMS = 2,

    /// <summary>
    /// In-app push notification
    /// </summary>
    Push = 3,

    /// <summary>
    /// In-app notification (read in app)
    /// </summary>
    InApp = 4,

    /// <summary>
    /// Multiple channels
    /// </summary>
    All = 99
}

/// <summary>
/// Notification priority levels
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Low priority - can be delayed
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority
    /// </summary>
    Normal = 2,

    /// <summary>
    /// High priority - should be sent immediately
    /// </summary>
    High = 3,

    /// <summary>
    /// Urgent - critical notification
    /// </summary>
    Urgent = 4
}
