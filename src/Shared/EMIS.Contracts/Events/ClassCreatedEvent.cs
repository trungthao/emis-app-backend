using EMIS.EventBus.Abstractions;

namespace EMIS.Contracts.Events
{
    /// <summary>
    /// Event fired when a new class is created in ClassService/StudentService
    /// TeacherService subscribes to this event to maintain local replica
    /// </summary>
    public class ClassCreatedEvent : BaseEvent
    {
        public override string EventType => "emis.class.created";

        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public string? AcademicYear { get; set; }
        public int? TotalStudents { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? HomeroomTeacherId { get; set; }
    }
}
