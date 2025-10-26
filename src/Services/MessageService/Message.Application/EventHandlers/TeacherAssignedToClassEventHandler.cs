using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Message.Domain.Entities;
using Message.Domain.Enums;
using Message.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Message.Application.EventHandlers;

/// <summary>
/// Event handler khi giáo viên được phân công vào lớp
/// Thêm giáo viên vào các nhóm chat của học sinh trong lớp
/// </summary>
public class TeacherAssignedToClassEventHandler : IEventHandler<TeacherAssignedToClassEvent>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<TeacherAssignedToClassEventHandler> _logger;

    public TeacherAssignedToClassEventHandler(
        IConversationRepository conversationRepository,
        ILogger<TeacherAssignedToClassEventHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task HandleAsync(TeacherAssignedToClassEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Handling TeacherAssignedToClassEvent for Teacher {TeacherId} in Class {ClassId}",
                @event.TeacherId,
                @event.ClassId);

            // TODO: Lấy danh sách tất cả student groups trong class này
            // Và thêm giáo viên vào các nhóm chat
            // Cần có API từ StudentService để lấy danh sách học sinh trong lớp

            _logger.LogInformation(
                "Teacher {TeacherId} assigned to Class {ClassId}",
                @event.TeacherId,
                @event.ClassId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling TeacherAssignedToClassEvent for Teacher {TeacherId}",
                @event.TeacherId);
            throw;
        }
    }
}
