using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Teacher.Domain.Repositories;

namespace Teacher.Application.EventHandlers
{
    /// <summary>
    /// Handles ClassCreatedEvent to maintain local replica of class information
    /// Pattern: Event-Driven Architecture + Eventual Consistency
    /// Purpose: Avoid N+1 queries and service-to-service synchronous calls (production scalability)
    /// </summary>
    public class ClassCreatedEventHandler : IEventHandler<ClassCreatedEvent>
    {
        private readonly ITeacherRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClassCreatedEventHandler> _logger;

        public ClassCreatedEventHandler(
            ITeacherRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ClassCreatedEventHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(ClassCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Handling ClassCreatedEvent for ClassId: {ClassId}, ClassName: {ClassName}",
                @event.ClassId,
                @event.ClassName);

            try
            {
                // Create local replica of class information
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
                    "Successfully synced ClassInfo for ClassId: {ClassId} to local replica",
                    @event.ClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to handle ClassCreatedEvent for ClassId: {ClassId}. Error: {ErrorMessage}",
                    @event.ClassId,
                    ex.Message);

                // Re-throw to prevent Kafka offset commit
                // Message will be retried
                throw;
            }
        }
    }
}
