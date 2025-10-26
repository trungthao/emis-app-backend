using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Teacher.Application.DTOs;
using Teacher.Application.UseCases.Teachers.Commands.CreateTeacher;
using Teacher.Application.UseCases.Teachers.Queries.GetTeacherDetail;

namespace Teacher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(IMediator mediator, ILogger<TeachersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous] // Allow unauthenticated access for testing EventBus flow
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TeacherDto>> Create([FromBody] CreateTeacherCommand command)
    {
        _logger.LogInformation("Creating teacher {FirstName} {LastName}", command.FirstName, command.LastName);
        var dto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeacherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherDetailDto>> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting teacher detail for ID: {TeacherId}", id);
            var query = new GetTeacherDetailQuery { TeacherId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Teacher not found: {TeacherId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeacherDto>>> GetAll()
    {
        // TODO: implement GetAllTeachersQuery
        return Ok();
    }

    [HttpPost("{id:guid}/assignments")]
    [ProducesResponseType(typeof(Teacher.Application.DTOs.TeacherAssignmentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<Teacher.Application.DTOs.TeacherAssignmentDto>> AssignClass(Guid id, [FromBody] Teacher.Application.UseCases.Teachers.Commands.AssignClass.AssignClassToTeacherCommand command)
    {
        if (id != command.TeacherId) return BadRequest("TeacherId in URL and payload must match");
        var dto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = command.TeacherId }, dto);
    }

    [HttpDelete("assignments/{assignmentId:guid}")]
    public async Task<IActionResult> UnassignClass(Guid assignmentId)
    {
        var cmd = new Teacher.Application.UseCases.Teachers.Commands.UnassignClass.UnassignClassFromTeacherCommand { AssignmentId = assignmentId };
        await _mediator.Send(cmd);
        return NoContent();
    }

    [HttpGet("{id:guid}/assignments")]
    [ProducesResponseType(typeof(IEnumerable<Teacher.Application.DTOs.TeacherAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Teacher.Application.DTOs.TeacherAssignmentDto>>> GetAssignments(Guid id)
    {
        // Querying directly via mediator queries not implemented; call repository via controller mediator pattern could be added.
        // For now, use a simple internal mediator query placeholder (not implemented) and return NotFound.
        return NotFound();
    }
}
