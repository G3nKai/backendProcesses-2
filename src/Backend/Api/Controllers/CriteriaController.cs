using Api.Authentication;
using Application.Criteria.Contracts;
using Application.Criteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class CriteriaController : ControllerBase
{
    private readonly ICriteriaService _criteriaService;

    public CriteriaController(ICriteriaService criteriaService)
    {
        _criteriaService = criteriaService;
    }

    [HttpGet("tasks/{taskId:guid}/criteria")]
    [ProducesResponseType(typeof(IReadOnlyList<CriterionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTaskCriteria([FromRoute] Guid taskId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized(CreateUnauthorized());
        }

        var criteria = await _criteriaService.GetTaskCriteriaAsync(userId.Value, taskId, cancellationToken);
        return Ok(criteria);
    }

    [HttpPost("tasks/{taskId:guid}/criteria")]
    [ProducesResponseType(typeof(CriterionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromRoute] Guid taskId, [FromBody] CreateCriterionRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized(CreateUnauthorized());
        }

        var result = await _criteriaService.CreateAsync(userId.Value, taskId, request ?? new CreateCriterionRequest(), cancellationToken);

        return result.Status switch
        {
            CriterionCreateStatus.Success => CreatedAtAction(nameof(GetTaskCriteria), new { taskId }, result.Criterion),
            CriterionCreateStatus.NotFound => NotFound(CreateNotFound()),
            CriterionCreateStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateForbidden()),
            _ => throw new InvalidOperationException("Unsupported criteria create status.")
        };
    }

    [HttpPatch("criteria/{id:guid}")]
    [ProducesResponseType(typeof(CriterionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCriterionRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized(CreateUnauthorized());
        }

        var result = await _criteriaService.UpdateAsync(userId.Value, id, request ?? new UpdateCriterionRequest(), cancellationToken);

        return result.Status switch
        {
            CriterionUpdateStatus.Success => Ok(result.Criterion),
            CriterionUpdateStatus.NotFound => NotFound(CreateNotFound()),
            CriterionUpdateStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateForbidden()),
            _ => throw new InvalidOperationException("Unsupported criteria update status.")
        };
    }

    [HttpDelete("criteria/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized(CreateUnauthorized());
        }

        var result = await _criteriaService.DeleteAsync(userId.Value, id, cancellationToken);

        return result.Status switch
        {
            CriterionDeleteStatus.Success => NoContent(),
            CriterionDeleteStatus.NotFound => NotFound(CreateNotFound()),
            CriterionDeleteStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateForbidden()),
            _ => throw new InvalidOperationException("Unsupported criteria delete status.")
        };
    }

    private static ProblemDetails CreateUnauthorized()
    {
        return new ProblemDetails
        {
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "Authentication failed."
        };
    }

    private static ProblemDetails CreateForbidden()
    {
        return new ProblemDetails
        {
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = "Access denied."
        };
    }

    private static ProblemDetails CreateNotFound()
    {
        return new ProblemDetails
        {
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = "Criterion not found."
        };
    }
}
