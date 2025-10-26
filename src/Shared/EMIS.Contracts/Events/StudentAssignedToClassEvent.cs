using EMIS.Contracts.Events;

namespace EMIS.Contracts.Events;

/// <summary>
/// Event khi học sinh được phân lớp
/// </summary>
public class StudentAssignedToClassEvent : BaseEvent
{
    public override string EventType => nameof(StudentAssignedToClassEvent);

    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public List<Guid> ParentIds { get; set; } = new();
    public List<Guid> TeacherIds { get; set; } = new();
}
