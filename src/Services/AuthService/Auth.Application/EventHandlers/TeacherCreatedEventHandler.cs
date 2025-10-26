using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Auth.Application.DTOs;
using Auth.Application.UseCases.Register;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.EventHandlers
{
    /// <summary>
    /// Handles TeacherCreatedEvent to automatically create user account for new teacher
    /// </summary>
    public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TeacherCreatedEventHandler> _logger;

        public TeacherCreatedEventHandler(
            IMediator mediator,
            ILogger<TeacherCreatedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Handling TeacherCreatedEvent for Teacher {TeacherId} with email {Email}",
                @event.TeacherId,
                @event.Email);

            try
            {
                // Use email as username, fallback to phone if email is empty
                var username = !string.IsNullOrWhiteSpace(@event.Email) 
                    ? @event.Email 
                    : @event.PhoneNumber ?? string.Empty;

                // Validate username is not empty
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogError(
                        "Cannot create account for Teacher {TeacherId}: both Email and PhoneNumber are empty",
                        @event.TeacherId);
                    return;
                }

                // Create register command
                var registerCommand = new RegisterCommand(
                    Username: username,
                    Email: @event.Email,
                    Password: @event.DefaultPassword,
                    FullName: @event.FullName,
                    PhoneNumber: @event.PhoneNumber ?? string.Empty,
                    Roles: new List<string> { "Teacher" }
                );

                // Execute registration via MediatR
                var result = await _mediator.Send(registerCommand, cancellationToken);

                _logger.LogInformation(
                    "Successfully created user account for Teacher {TeacherId}. Username: {Username}",
                    @event.TeacherId,
                    result.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create user account for Teacher {TeacherId}. Error: {ErrorMessage}",
                    @event.TeacherId,
                    ex.Message);

                // Re-throw to prevent Kafka offset commit
                // This message will be retried
                throw;
            }
        }
    }
}
