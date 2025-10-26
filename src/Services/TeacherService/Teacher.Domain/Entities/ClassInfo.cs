using Teacher.Domain.Common;

namespace Teacher.Domain.Entities
{
    /// <summary>
    /// Local replica of Class information from StudentService/ClassService
    /// Updated via event-driven architecture (ClassCreated, ClassUpdated events)
    /// Pattern: Eventual Consistency + Local Cache
    /// </summary>
    public class ClassInfo : BaseEntity
    {
        public Guid ClassId { get; private set; } // External ClassId from ClassService
        public string ClassName { get; private set; } = string.Empty;
        public string? Grade { get; private set; }
        public string? AcademicYear { get; private set; }
        public int? TotalStudents { get; private set; }
        public Guid? SchoolId { get; private set; }
        
        // Metadata for sync
        public DateTime LastSyncedAt { get; private set; }
        public string? SyncSource { get; private set; } // "ClassService", "StudentService"
        
        private ClassInfo() { }

        public ClassInfo(
            Guid classId,
            string className,
            string? grade = null,
            string? academicYear = null,
            int? totalStudents = null,
            Guid? schoolId = null,
            string? syncSource = null)
        {
            ClassId = classId;
            ClassName = className;
            Grade = grade;
            AcademicYear = academicYear;
            TotalStudents = totalStudents;
            SchoolId = schoolId;
            LastSyncedAt = DateTime.UtcNow;
            SyncSource = syncSource;
        }

        public void UpdateInfo(
            string className,
            string? grade = null,
            string? academicYear = null,
            int? totalStudents = null,
            Guid? schoolId = null)
        {
            ClassName = className;
            Grade = grade;
            AcademicYear = academicYear;
            TotalStudents = totalStudents;
            SchoolId = schoolId;
            LastSyncedAt = DateTime.UtcNow;
        }

        public void UpdateStudentCount(int totalStudents)
        {
            TotalStudents = totalStudents;
            LastSyncedAt = DateTime.UtcNow;
        }
    }
}
