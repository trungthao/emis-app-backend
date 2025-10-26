using Teacher.Domain.Common;

namespace Teacher.Domain.Entities
{
    /// <summary>
    /// Relationship entity between Teacher and Class (stores assignment history)
    /// TeacherService keeps references to classes by Id (aggregate reference)
    /// </summary>
    public class TeacherClassAssignment : BaseEntity
    {
        public Guid TeacherId { get; private set; }
        public Guid ClassId { get; private set; }
        public DateTime AssignedAt { get; private set; }
        public string? Role { get; private set; } // optional role like "Homeroom"

        private TeacherClassAssignment() { }

        public TeacherClassAssignment(Guid teacherId, Guid classId, DateTime? assignedAt = null, string? role = null)
        {
            TeacherId = teacherId;
            ClassId = classId;
            AssignedAt = assignedAt ?? DateTime.UtcNow;
            Role = role;
        }

        public void UpdateRole(string? role) => Role = role;
    }
}
