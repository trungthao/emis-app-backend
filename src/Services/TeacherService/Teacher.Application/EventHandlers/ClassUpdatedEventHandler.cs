using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Teacher.Domain.Repositories;

namespace Teacher.Application.EventHandlers
{
    /// <summary>
    /// Handles ClassUpdatedEvent to keep local replica synchronized
    /// Pattern: Event-Driven Architecture + Eventual Consistency
    /// </summary>
    public class ClassUpdatedEventHandler : IEventHandler<ClassUpdatedEvent>
    {
        private readonly ITeacherRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClassUpdatedEventHandler> _logger;

        public ClassUpdatedEventHandler(
            ITeacherRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ClassUpdatedEventHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(ClassUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Handling ClassUpdatedEvent for ClassId: {ClassId}, ClassName: {ClassName}",
                @event.ClassId,
                @event.ClassName);

            try
            {
                // Upsert (update or insert) class information in local replica
                var classInfo = new Teacher.Domain.Entities.ClassInfo(
                    classId: @event.ClassId,
                    className: @event.ClassName,
                    grade: @event.Grade,
                    academicYear: @event.AcademicYear,
                    totalStudents: @event.TotalStudents,
                    schoolId: @event.SchoolId,
                    syncSource: "ClassService"
                );

                await _repository.UpsertClassInfoAsync(classInfo, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully updated ClassInfo for ClassId: {ClassId} in local replica",
                    @event.ClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to handle ClassUpdatedEvent for ClassId: {ClassId}. Error: {ErrorMessage}",
                    @event.ClassId,
                    ex.Message);

                // Re-throw to prevent Kafka offset commit
                throw;
            }
        }
    }
}
