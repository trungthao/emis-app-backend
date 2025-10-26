namespace EMIS.Contracts.Constants;

/// <summary>
/// Standardized error codes across all EMIS services
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Authentication & Authorization Errors (AUTH_xxx)
    /// </summary>
    public static class Auth
    {
        public const string UserNotFound = "AUTH_001";
        public const string InvalidCredentials = "AUTH_002";
        public const string InvalidToken = "AUTH_003";
        public const string TokenExpired = "AUTH_004";
        public const string InsufficientPermissions = "AUTH_005";
        public const string UserAlreadyExists = "AUTH_006";
        public const string InvalidRefreshToken = "AUTH_007";
        public const string AccountDeactivated = "AUTH_008";
        public const string AccountLocked = "AUTH_009";
        public const string PasswordTooWeak = "AUTH_010";
    }

    /// <summary>
    /// Student Management Errors (STU_xxx)
    /// </summary>
    public static class Student
    {
        public const string NotFound = "STU_001";
        public const string AlreadyEnrolled = "STU_002";
        public const string EnrollmentFailed = "STU_003";
        public const string InvalidGrade = "STU_004";
        public const string ClassFull = "STU_005";
        public const string TransferNotAllowed = "STU_006";
    }

    /// <summary>
    /// Teacher Management Errors (TCH_xxx)
    /// </summary>
    public static class Teacher
    {
        public const string NotFound = "TCH_001";
        public const string InvalidSubject = "TCH_002";
        public const string AssignmentConflict = "TCH_003";
        public const string MaxClassesReached = "TCH_004";
    }

    /// <summary>
    /// Attendance Errors (ATT_xxx)
    /// </summary>
    public static class Attendance
    {
        public const string RecordNotFound = "ATT_001";
        public const string AlreadyMarked = "ATT_002";
        public const string InvalidStatus = "ATT_003";
        public const string DateOutOfRange = "ATT_004";
    }

    /// <summary>
    /// Grade Errors (GRD_xxx)
    /// </summary>
    public static class Grade
    {
        public const string NotFound = "GRD_001";
        public const string InvalidScore = "GRD_002";
        public const string AlreadyFinalized = "GRD_003";
        public const string NotAuthorized = "GRD_004";
    }

    /// <summary>
    /// Notification Errors (NTF_xxx)
    /// </summary>
    public static class Notification
    {
        public const string SendFailed = "NTF_001";
        public const string InvalidRecipient = "NTF_002";
        public const string TemplateNotFound = "NTF_003";
        public const string QuotaExceeded = "NTF_004";
    }

    /// <summary>
    /// Validation Errors (VAL_xxx)
    /// </summary>
    public static class Validation
    {
        public const string InvalidEmail = "VAL_001";
        public const string InvalidPhoneNumber = "VAL_002";
        public const string RequiredFieldMissing = "VAL_003";
        public const string InvalidDateRange = "VAL_004";
        public const string InvalidFormat = "VAL_005";
    }

    /// <summary>
    /// System Errors (SYS_xxx)
    /// </summary>
    public static class System
    {
        public const string InternalError = "SYS_001";
        public const string ServiceUnavailable = "SYS_002";
        public const string DatabaseError = "SYS_003";
        public const string CacheError = "SYS_004";
        public const string MessageBrokerError = "SYS_005";
    }
}
