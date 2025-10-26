namespace EMIS.Contracts.Constants;

/// <summary>
/// Redis cache key prefixes and formatters
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Default cache expiration times
    /// </summary>
    public static class Expiration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(2);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
    }

    /// <summary>
    /// User & Authentication cache keys
    /// </summary>
    public static class Auth
    {
        private const string Prefix = "auth";

        public static string UserInfo(Guid userId) => $"{Prefix}:user:info:{userId}";
        public static string UserPermissions(Guid userId) => $"{Prefix}:user:permissions:{userId}";
        public static string UserRoles(Guid userId) => $"{Prefix}:user:roles:{userId}";
        public static string TokenBlacklist(string tokenId) => $"{Prefix}:token:blacklist:{tokenId}";
    }

    /// <summary>
    /// Student cache keys
    /// </summary>
    public static class Student
    {
        private const string Prefix = "student";

        public static string Profile(Guid studentId) => $"{Prefix}:profile:{studentId}";
        public static string Enrollment(Guid studentId) => $"{Prefix}:enrollment:{studentId}";
        public static string ClassStudents(Guid classId) => $"{Prefix}:class:{classId}:students";
        public static string GradeStudents(string grade) => $"{Prefix}:grade:{grade}:students";
    }

    /// <summary>
    /// Teacher cache keys
    /// </summary>
    public static class Teacher
    {
        private const string Prefix = "teacher";

        public static string Profile(Guid teacherId) => $"{Prefix}:profile:{teacherId}";
        public static string Assignments(Guid teacherId) => $"{Prefix}:assignments:{teacherId}";
        public static string ClassTeachers(Guid classId) => $"{Prefix}:class:{classId}:teachers";
    }

    /// <summary>
    /// Attendance cache keys
    /// </summary>
    public static class Attendance
    {
        private const string Prefix = "attendance";

        public static string DailyRecord(Guid classId, DateOnly date) 
            => $"{Prefix}:daily:{classId}:{date:yyyy-MM-dd}";
        public static string StudentAttendance(Guid studentId, int year, int month)
            => $"{Prefix}:student:{studentId}:{year}:{month}";
    }

    /// <summary>
    /// Grade cache keys
    /// </summary>
    public static class Grade
    {
        private const string Prefix = "grade";

        public static string StudentGrades(Guid studentId, string semester)
            => $"{Prefix}:student:{studentId}:{semester}";
        public static string ClassGrades(Guid classId, string subject)
            => $"{Prefix}:class:{classId}:{subject}";
    }

    /// <summary>
    /// System-wide cache keys
    /// </summary>
    public static class System
    {
        private const string Prefix = "system";

        public static string Configuration(string key) => $"{Prefix}:config:{key}";
        public static string FeatureFlag(string featureName) => $"{Prefix}:feature:{featureName}";
    }
}
