using MediatR;

namespace Teacher.Application.UseCases.Teachers.Commands.UnassignClass
{
    public class UnassignClassFromTeacherCommand : IRequest<bool>
    {
        public Guid AssignmentId { get; set; }
    }
}
