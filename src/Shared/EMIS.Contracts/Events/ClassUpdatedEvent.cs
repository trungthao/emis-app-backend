using EMIS.EventBus.Abstractions;

namespace EMIS.Contracts.Events
{
    /// <summary>
    /// Event fired when class information is updated in ClassService/StudentService
    /// TeacherService subscribes to this event to keep local replica in sync
    /// </summary>
    public class ClassUpdatedEvent : BaseEvent
    {
        public override string EventType => "emis.class.updated";

        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public string? AcademicYear { get; set; }
        public int? TotalStudents { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? HomeroomTeacherId { get; set; }
    }
}
