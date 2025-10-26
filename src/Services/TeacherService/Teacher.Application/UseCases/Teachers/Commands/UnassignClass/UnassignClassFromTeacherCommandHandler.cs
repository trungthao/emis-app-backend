using MediatR;
using Teacher.Domain.Repositories;

namespace Teacher.Application.UseCases.Teachers.Commands.UnassignClass
{
    public class UnassignClassFromTeacherCommandHandler : IRequestHandler<UnassignClassFromTeacherCommand, bool>
    {
        private readonly ITeacherRepository _repository;

        public UnassignClassFromTeacherCommandHandler(ITeacherRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UnassignClassFromTeacherCommand request, CancellationToken cancellationToken)
        {
            await _repository.RemoveAssignmentAsync(request.AssignmentId, cancellationToken);
            return true;
        }
    }
}
