using Microsoft.AspNetCore.Mvc;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;

namespace RequestsManagement.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Produces("application/json")]
public sealed class RequestsController(IRequestQueryService queryService) : ControllerBase
{
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
}
