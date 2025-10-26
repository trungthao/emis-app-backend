using MediatR;
using Teacher.Application.DTOs;
using Teacher.Domain.Repositories;

namespace Teacher.Application.UseCases.Teachers.Commands.AssignClass
{
    public class AssignClassToTeacherCommandHandler : IRequestHandler<AssignClassToTeacherCommand, TeacherAssignmentDto>
    {
        private readonly ITeacherRepository _repository;

        public AssignClassToTeacherCommandHandler(ITeacherRepository repository)
        {
            _repository = repository;
        }

        public async Task<TeacherAssignmentDto> Handle(AssignClassToTeacherCommand request, CancellationToken cancellationToken)
        {
            var assignment = new Teacher.Domain.Entities.TeacherClassAssignment(request.TeacherId, request.ClassId, DateTime.UtcNow, request.Role);
            await _repository.AddAssignmentAsync(assignment, cancellationToken);

            return new TeacherAssignmentDto
            {
                Id = assignment.Id,
                TeacherId = assignment.TeacherId,
                ClassId = assignment.ClassId,
                AssignedAt = assignment.AssignedAt,
                Role = assignment.Role
            };
        }
    }
}
