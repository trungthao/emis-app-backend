using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;
using Message.Application.Commands;
using Message.Domain.Enums;
using Message.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Message.Application.EventHandlers;

/// <summary>
/// Event handler khi học sinh được phân lớp
/// Tự động tạo nhóm chat cho học sinh
/// </summary>
public class StudentAssignedToClassEventHandler : IEventHandler<StudentAssignedToClassEvent>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<StudentAssignedToClassEventHandler> _logger;

    public StudentAssignedToClassEventHandler(
        IConversationRepository conversationRepository,
        IMediator mediator,
        ILogger<StudentAssignedToClassEventHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(StudentAssignedToClassEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Handling StudentAssignedToClassEvent for Student {StudentId} in Class {ClassId}",
                @event.StudentId,
                @event.ClassId);

            // Kiểm tra xem nhóm chat của học sinh đã tồn tại chưa
            var existingConversation = await _conversationRepository.FindStudentGroupAsync(
                @event.StudentId,
                cancellationToken);

            if (existingConversation != null)
            {
                _logger.LogInformation(
                    "Student group conversation already exists for Student {StudentId}",
                    @event.StudentId);
                return;
            }

            // Tạo danh sách member IDs (phụ huynh + giáo viên)
            var memberIds = new List<string>();
            
            // Thêm phụ huynh
            foreach (var parentId in @event.ParentIds)
            {
                memberIds.Add(parentId.ToString());
            }
            
            // Thêm giáo viên
            foreach (var teacherId in @event.TeacherIds)
            {
                memberIds.Add(teacherId.ToString());
            }

            // Tạo nhóm chat cho học sinh
            var command = new CreateConversationCommand
            {
                Type = ConversationType.StudentGroup,
                GroupName = $"Nhóm chat của học sinh {@event.StudentName}",
                StudentId = @event.StudentId,
                ClassId = @event.ClassId,
                MemberIds = memberIds,
                CreatedBy = "System" // System tự động tạo
            };

            await _mediator.Send(command);

            _logger.LogInformation(
                "Successfully created student group conversation for Student {StudentId}",
                @event.StudentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling StudentAssignedToClassEvent for Student {StudentId}",
                @event.StudentId);
            throw;
        }
    }
}
