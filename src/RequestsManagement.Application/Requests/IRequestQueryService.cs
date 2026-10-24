using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;

namespace RequestsManagement.Application.Requests;

public interface IRequestQueryService
{
    Task<PagedResult<RequestDto>> GetPagedAsync(RequestQueryParameters parameters, CancellationToken cancellationToken);

    Task<RequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<RequestsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
