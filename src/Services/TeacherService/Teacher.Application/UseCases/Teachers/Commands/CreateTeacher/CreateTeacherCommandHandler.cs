using MediatR;
using Teacher.Application.DTOs;
using Teacher.Domain.Entities;
using Teacher.Domain.Repositories;
using EMIS.EventBus.Abstractions;
using EMIS.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Teacher.Application.UseCases.Teachers.Commands.CreateTeacher
{
    public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherDto>
    {
        private readonly ITeacherRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateTeacherCommandHandler> _logger;

        public CreateTeacherCommandHandler(
            ITeacherRepository repository, 
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            ILogger<CreateTeacherCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<TeacherDto> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
        {
            // Basic validation could be added here
            if (!string.IsNullOrEmpty(request.Phone))
            {
                var exists = await _repository.IsPhoneExistsAsync(request.Phone, cancellationToken);
                if (exists)
                {
                    throw new InvalidOperationException("Phone number already exists");
                }
            }

            var teacher = new Teacher.Domain.Entities.Teacher(request.FirstName, request.LastName, request.Phone);
            teacher.UpdateBasicInfo(request.FirstName, request.LastName, request.DateOfBirth, request.Phone, request.Email, request.Address);

            await _repository.AddAsync(teacher, cancellationToken);

            // Create assignments if provided
            var assignmentDtos = new List<TeacherAssignmentDto>();
            if (request.Assignments != null && request.Assignments.Any())
            {
                foreach (var assignmentInput in request.Assignments)
                {
                    var assignment = new TeacherClassAssignment(teacher.Id, assignmentInput.ClassId, DateTime.UtcNow, assignmentInput.Role);
                    await _repository.AddAssignmentAsync(assignment, cancellationToken);
                    
                    assignmentDtos.Add(new TeacherAssignmentDto
                    {
                        Id = assignment.Id,
                        TeacherId = assignment.TeacherId,
                        ClassId = assignment.ClassId,
                        AssignedAt = assignment.AssignedAt,
                        Role = assignment.Role
                    });
                }
            }

            // Save all changes in one transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish TeacherCreatedEvent for account creation
            var defaultPassword = GenerateDefaultPassword(teacher);
            var teacherCreatedEvent = new TeacherCreatedEvent
            {
                TeacherId = teacher.Id,
                FullName = teacher.FullName,
                Email = teacher.Email ?? string.Empty,
                PhoneNumber = teacher.Phone,
                Subject = null, // Not tracked in current model
                DateOfBirth = teacher.DateOfBirth,
                DefaultPassword = defaultPassword,
                SchoolId = null // Not tracked in current model
            };

            await _eventBus.PublishAsync(teacherCreatedEvent, cancellationToken);
            _logger.LogInformation(
                "Published TeacherCreatedEvent for Teacher {TeacherId} with email {Email}", 
                teacher.Id, 
                teacher.Email);

            return new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                FullName = teacher.FullName,
                DateOfBirth = teacher.DateOfBirth,
                Phone = teacher.Phone,
                Email = teacher.Email,
                Address = teacher.Address,
                AvatarUrl = teacher.AvatarUrl,
                Status = teacher.Status.ToString(),
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt,
                Assignments = assignmentDtos.Any() ? assignmentDtos : null
            };
        }

        /// <summary>
        /// Generates a default password for new teacher account
        /// Format: Teacher@[first8chars of GUID]
        /// </summary>
        private string GenerateDefaultPassword(Teacher.Domain.Entities.Teacher teacher)
        {
            var randomPart = Guid.NewGuid().ToString("N")[..8];
            return $"Teacher@{randomPart}";
        }
    }
}
