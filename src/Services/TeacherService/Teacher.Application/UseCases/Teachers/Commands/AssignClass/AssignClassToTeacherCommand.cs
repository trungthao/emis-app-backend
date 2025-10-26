using MediatR;
using Teacher.Application.DTOs;

namespace Teacher.Application.UseCases.Teachers.Commands.AssignClass
{
    public class AssignClassToTeacherCommand : IRequest<TeacherAssignmentDto>
    {
        public Guid TeacherId { get; set; }
        public Guid ClassId { get; set; }
        public string? Role { get; set; }
    }
}
