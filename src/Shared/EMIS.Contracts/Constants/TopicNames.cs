namespace EMIS.Contracts.Constants;

/// <summary>
/// Kafka topic names for event-driven communication
/// </summary>
public static class TopicNames
{
    /// <summary>
    /// Teacher Management Topics
    /// Published by: TeacherService
    /// Consumed by: AuthService, NotificationService, AnalyticsService, etc.
    /// </summary>
    public static class Teacher
    {
        public const string Created = "emis.teacher.created";
        public const string Updated = "emis.teacher.updated";
        public const string Deleted = "emis.teacher.deleted";
        public const string AssignmentCreated = "emis.teacher.assignment.created";
        public const string AssignmentUpdated = "emis.teacher.assignment.updated";
        public const string ClassAssigned = "emis.teacher.class.assigned";
    }

    /// <summary>
    /// Student Management Topics
    /// Published by: StudentService
    /// Consumed by: AuthService, NotificationService, AnalyticsService, etc.
    /// </summary>
    public static class Student
    {
        public const string Created = "emis.student.created";
        public const string Updated = "emis.student.updated";
        public const string Deleted = "emis.student.deleted";
        public const string Enrolled = "emis.student.enrolled";
        public const string Transferred = "emis.student.transferred";
        public const string Graduated = "emis.student.graduated";
        public const string StatusChanged = "emis.student.status.changed";
    }

    /// <summary>
    /// Class Management Topics
    /// Published by: ClassService
    /// Consumed by: TeacherService, StudentService (for local replica sync)
    /// </summary>
    public static class Class
    {
        public const string Created = "emis.class.created";
        public const string Updated = "emis.class.updated";
        public const string Deleted = "emis.class.deleted";
    }

    /// <summary>
    /// Parent Management Topics
    /// Published by: ParentService
    /// Consumed by: AuthService, NotificationService, StudentService (for relationships)
    /// </summary>
    public static class Parent
    {
        public const string Created = "emis.parent.created";
        public const string Updated = "emis.parent.updated";
        public const string Deleted = "emis.parent.deleted";
        public const string ChildLinked = "emis.parent.child.linked";
        public const string ChildUnlinked = "emis.parent.child.unlinked";
    }

    /// <summary>
    /// Authentication & User Management Topics
    /// Published by: AuthService
    /// Consumed by: AuditService, NotificationService, etc.
    /// </summary>
    public static class Auth
    {
        public const string UserPasswordChanged = "emis.auth.user.password.changed";
        public const string UserRoleUpdated = "emis.auth.user.role.updated";
        public const string UserDeactivated = "emis.auth.user.deactivated";
        public const string LoginAttemptFailed = "emis.auth.login.failed";
        public const string LoginSuccessful = "emis.auth.login.successful";
    }

    /// <summary>
    /// Attendance Topics
    /// </summary>
    public static class Attendance
    {
        public const string Marked = "emis.attendance.marked";
        public const string AbsenceReported = "emis.attendance.absence.reported";
        public const string LateArrival = "emis.attendance.late.arrival";
    }

    /// <summary>
    /// Grade Topics
    /// </summary>
    public static class Grade
    {
        public const string Published = "emis.grade.published";
        public const string Updated = "emis.grade.updated";
        public const string Finalized = "emis.grade.finalized";
    }

    /// <summary>
    /// Notification Topics
    /// </summary>
    public static class Notification
    {
        public const string EmailQueued = "emis.notification.email.queued";
        public const string SMSQueued = "emis.notification.sms.queued";
        public const string PushNotificationQueued = "emis.notification.push.queued";
        public const string NotificationSent = "emis.notification.sent";
        public const string NotificationFailed = "emis.notification.failed";
    }

    /// <summary>
    /// Dead Letter Queue Topics
    /// </summary>
    public static class DeadLetter
    {
        public const string AuthEvents = "emis.dlq.auth";
        public const string StudentEvents = "emis.dlq.student";
        public const string NotificationEvents = "emis.dlq.notification";
    }
}
