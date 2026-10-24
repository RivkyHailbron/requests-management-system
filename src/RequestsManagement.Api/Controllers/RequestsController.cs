using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests;
using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Produces("application/json")]
public sealed class RequestsController(
    IRequestQueryService queryService,
    IRequestStatusService statusService) : ControllerBase
{
    private const string CurrentRequestExtension = "currentRequest";
    private const string AllowedStatusesExtension = "allowedStatuses";

    [HttpGet]
    [ProducesResponseType<PagedResult<RequestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<RequestDto>>> GetRequestsAsync(
        [FromQuery] RequestQueryParameters parameters,
        CancellationToken cancellationToken) =>
        Ok(await queryService.GetPagedAsync(parameters, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> GetRequestAsync(int id, CancellationToken cancellationToken)
    {
        var request = await queryService.GetByIdAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpGet("summary")]
    [ProducesResponseType<RequestsSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RequestsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken) =>
        Ok(await queryService.GetSummaryAsync(cancellationToken));

    [HttpGet("status-transitions")]
    [ProducesResponseType<IReadOnlyDictionary<RequestStatus, IReadOnlySet<RequestStatus>>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyDictionary<RequestStatus, IReadOnlySet<RequestStatus>>> GetStatusTransitions() =>
        Ok(Enum.GetValues<RequestStatus>().ToDictionary(
            status => status,
            RequestStatusTransitions.GetAllowedTargets));

    [HttpGet("{id:int}/history")]
    [ProducesResponseType<IReadOnlyList<RequestStatusHistoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RequestStatusHistoryDto>>> GetHistoryAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var history = await queryService.GetHistoryAsync(id, cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPatch("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RequestDto>> UpdateStatusAsync(
        int id,
        UpdateRequestStatusRequest command,
        [FromHeader(Name = ApiHeaders.UserId), Required, StringLength(RequestConstraints.UserIdentifierMaxLength)] string userId,
        CancellationToken cancellationToken)
    {
        var result = await statusService.UpdateStatusAsync(id, command, userId.Trim(), cancellationToken);

        return result.Outcome switch
        {
            StatusUpdateOutcome.Updated => Ok(result.Request),
            StatusUpdateOutcome.Conflict => ProblemWithExtensions(
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The request was modified by another user. Reload it and try again.",
                new Dictionary<string, object?> { [CurrentRequestExtension] = result.Request }),
            StatusUpdateOutcome.InvalidTransition => ProblemWithExtensions(
                StatusCodes.Status422UnprocessableEntity,
                "Invalid status transition",
                $"A request in status '{result.Request!.Status}' cannot move to '{command.Status}'.",
                new Dictionary<string, object?>
                {
                    [AllowedStatusesExtension] = RequestStatusTransitions.GetAllowedTargets(result.Request.Status)
                }),
            _ => NotFound()
        };
    }

    [HttpPatch("status")]
    [Consumes("application/json")]
    [ProducesResponseType<BulkStatusUpdateResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkStatusUpdateResultDto>> BulkUpdateStatusAsync(
        BulkUpdateRequestStatusRequest command,
        [FromHeader(Name = ApiHeaders.UserId), Required, StringLength(RequestConstraints.UserIdentifierMaxLength)] string userId,
        CancellationToken cancellationToken) =>
        Ok(await statusService.BulkUpdateStatusAsync(command, userId.Trim(), cancellationToken));

    private ObjectResult ProblemWithExtensions(
        int statusCode,
        string title,
        string detail,
        IDictionary<string, object?> extensions)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode, title, detail: detail);
        foreach (var (key, value) in extensions)
        {
            problem.Extensions[key] = value;
        }

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
