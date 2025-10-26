using MediatR;
using Teacher.Application.DTOs;

namespace Teacher.Application.UseCases.Teachers.Commands.CreateTeacher
{
    public record CreateTeacherCommand : IRequest<TeacherDto>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public DateTime? DateOfBirth { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        public List<AssignmentInputDto>? Assignments { get; init; }
    }
}
