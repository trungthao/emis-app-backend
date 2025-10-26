namespace Teacher.Application.DTOs
{
    public class TeacherAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
        public Guid ClassId { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? Role { get; set; }
    }
}
