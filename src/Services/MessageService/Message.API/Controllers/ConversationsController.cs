using MediatR;
using Message.Application.Commands;
using Message.Application.DTOs;
using Message.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Message.API.Controllers;

/// <summary>
/// Controller cho quản lý Conversations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(IMediator mediator, ILogger<ConversationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách conversations của user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations(
        [FromQuery] string userId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var query = new GetConversationsQuery
        {
            UserId = userId,
            Skip = skip,
            Limit = limit
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Tạo conversation mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConversationDto>> CreateConversation(
        [FromBody] CreateConversationCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetConversations), new { userId = command.CreatedBy }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo conversation" });
        }
    }

    /// <summary>
    /// Lấy messages của một conversation
    /// </summary>
    [HttpGet("{conversationId}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(
        string conversationId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var query = new GetMessagesQuery
        {
            ConversationId = conversationId,
            Skip = skip,
            Limit = limit
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gửi tin nhắn (Write-Behind Pattern)
    /// API publish event to Kafka → Return 202 Accepted → Consumer batch write to MongoDB
    /// </summary>
    [HttpPost("{conversationId}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(
        string conversationId,
        [FromBody] SendMessageCommand command)
    {
        try
        {
            // Đảm bảo conversationId khớp
            if (command.ConversationId != conversationId)
            {
                return BadRequest(new { message = "ConversationId không khớp" });
            }

            var result = await _mediator.Send(command);

            // ✅ Return 202 Accepted (instead of 201 Created)
            // Message chưa được persist to MongoDB, chỉ mới publish to Kafka
            return Accepted(result); // 202 Accepted
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { message = "Có lỗi xảy ra khi gửi tin nhắn" });
        }
    }
}
