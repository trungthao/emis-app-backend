using MediatR;
using Microsoft.AspNetCore.Mvc;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Application.UseCases.Students.Commands.CreateStudent;
using Student.Application.UseCases.Students.Commands.UpdateStudent;
using Student.Application.UseCases.Students.Commands.AssignStudentToClass;
using Student.Application.UseCases.Students.Queries.GetStudentById;
using Student.Application.UseCases.Parents.Commands.AddParentToStudent;
using Student.Application.UseCases.Parents.Commands.UpdateParentRelationship;
using Student.Application.UseCases.Parents.Commands.RemoveParentFromStudent;

namespace Student.API.Controllers;

/// <summary>
/// Controller để quản lý học sinh
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IMediator mediator, ILogger<StudentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Tạo mới học sinh
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<StudentDto>>> CreateStudent(
        [FromBody] CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new student with code: {StudentCode}", command.StudentCode);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStudentById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Lấy thông tin học sinh theo Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StudentDto>>> GetStudentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting student by id: {StudentId}", id);

        var query = new GetStudentByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả học sinh
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<IEnumerable<StudentDto>>>> GetAllStudents(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all students");

        // TODO: Implement GetAllStudentsQuery
        return Ok();
    }

    /// <summary>
    /// Cập nhật thông tin học sinh
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StudentDto>>> UpdateStudent(
        Guid id,
        [FromBody] UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating student: {StudentId}", id);

        var commandWithId = command with { Id = id };
        var result = await _mediator.Send(commandWithId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Xóa học sinh (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> DeleteStudent(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting student: {StudentId}", id);

        // TODO: Implement DeleteStudentCommand
        return Ok();
    }

    /// <summary>
    /// Phân lớp cho học sinh
    /// </summary>
    [HttpPost("{id:guid}/assign-class")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<bool>>> AssignToClass(
        Guid id,
        [FromBody] AssignStudentToClassCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning student {StudentId} to class {ClassId}", id, command.ClassId);

        var commandWithId = command with { StudentId = id };
        var result = await _mediator.Send(commandWithId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Thêm phụ huynh cho học sinh
    /// </summary>
    [HttpPost("{id:guid}/parents")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<Guid>>> AddParentToStudent(
        Guid id,
        [FromBody] AddParentToStudentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding parent to student: {StudentId}", id);

        // Set StudentId from route
        var commandWithId = command with { StudentId = id };
        var result = await _mediator.Send(commandWithId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStudentById), new { id }, result);
    }

    /// <summary>
    /// Cập nhật mối quan hệ phụ huynh - học sinh
    /// </summary>
    [HttpPut("{studentId:guid}/parents/{parentId:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<bool>>> UpdateParentRelationship(
        Guid studentId,
        Guid parentId,
        [FromBody] UpdateParentRelationshipCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating parent {ParentId} relationship with student {StudentId}", parentId, studentId);

        var commandWithIds = command with { StudentId = studentId, ParentId = parentId };
        var result = await _mediator.Send(commandWithIds, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Xóa liên kết phụ huynh - học sinh
    /// </summary>
    [HttpDelete("{studentId:guid}/parents/{parentId:guid}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<bool>>> RemoveParentFromStudent(
        Guid studentId,
        Guid parentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing parent {ParentId} from student {StudentId}", parentId, studentId);

        var command = new RemoveParentFromStudentCommand { StudentId = studentId, ParentId = parentId };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
