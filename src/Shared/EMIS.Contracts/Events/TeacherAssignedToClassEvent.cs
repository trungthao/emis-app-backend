using EMIS.Contracts.Events;

namespace EMIS.Contracts.Events;

/// <summary>
/// Event khi giáo viên được phân công vào lớp
/// </summary>
public class TeacherAssignedToClassEvent : BaseEvent
{
    public override string EventType => nameof(TeacherAssignedToClassEvent);

    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public bool IsHeadTeacher { get; set; }
}
