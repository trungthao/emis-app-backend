using MediatR;
using Microsoft.AspNetCore.Mvc;
using Student.Application.Common;
using Student.Application.DTOs;
using Student.Application.UseCases.Classes.Queries.GetAllClasses;

namespace Student.API.Controllers;

/// <summary>
/// Controller để quản lý lớp học
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(IMediator mediator, ILogger<ClassesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả lớp học
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<ClassDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<IEnumerable<ClassDto>>>> GetAllClasses(
        [FromQuery] Guid? gradeId,
        [FromQuery] bool? onlyActive,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all classes");

        var query = new GetAllClassesQuery 
        { 
            GradeId = gradeId,
            OnlyActive = onlyActive 
        };
        
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin lớp học theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<ClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ClassDto>>> GetClassById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting class by id: {ClassId}", id);

        // TODO: Implement GetClassByIdQuery
        return Ok();
    }
}
