using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;
using RequestsManagement.Domain.Requests;
using RequestsManagement.Infrastructure.Persistence;

namespace RequestsManagement.Infrastructure.Requests;

internal sealed class RequestQueryService(
    RequestsDbContext dbContext,
    IOptions<PaginationOptions> paginationOptions) : IRequestQueryService
{
    public async Task<PagedResult<RequestDto>> GetPagedAsync(
        RequestQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var pageSize = parameters.PageSize ?? paginationOptions.Value.DefaultPageSize;
        var query = dbContext.Requests.AsNoTracking().ApplyFilters(parameters);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplySorting(parameters.SortBy, parameters.SortDirection)
            .Skip((parameters.Page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToDto()
            .ToListAsync(cancellationToken);

        return new PagedResult<RequestDto>(items, parameters.Page, pageSize, totalCount);
    }

    public Task<RequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Requests
            .AsNoTracking()
            .Where(request => request.Id == id)
            .ProjectToDto()
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<RequestStatusHistoryDto>?> GetHistoryAsync(
        int requestId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Requests.AnyAsync(request => request.Id == requestId, cancellationToken))
        {
            return null;
        }

        return await dbContext.RequestStatusHistory
            .AsNoTracking()
            .Where(entry => entry.RequestId == requestId)
            .OrderBy(entry => entry.ChangedAt)
            .ThenBy(entry => entry.Id)
            .Select(entry => new RequestStatusHistoryDto(
                entry.Id,
                entry.RequestId,
                entry.PreviousStatus,
                entry.NewStatus,
                entry.ChangedAt,
                entry.ChangedBy))
            .ToListAsync(cancellationToken);
    }

    public async Task<RequestsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var statusCounts = await dbContext.Requests
            .GroupBy(request => request.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Status, entry => entry.Count, cancellationToken);

        var priorityCounts = await dbContext.Requests
            .GroupBy(request => request.Priority)
            .Select(group => new { Priority = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Priority, entry => entry.Count, cancellationToken);

        return new RequestsSummaryDto(
            statusCounts.Values.Sum(),
            [.. Enum.GetValues<RequestStatus>()
                .Select(status => new StatusCountDto(status, statusCounts.GetValueOrDefault(status)))],
            [.. Enum.GetValues<RequestPriority>()
                .Select(priority => new PriorityCountDto(priority, priorityCounts.GetValueOrDefault(priority)))]);
    }
}
