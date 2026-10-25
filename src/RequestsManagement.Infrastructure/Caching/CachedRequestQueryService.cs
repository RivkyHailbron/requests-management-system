using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Application.Requests.Queries;

namespace RequestsManagement.Infrastructure.Caching;

internal sealed class CachedRequestQueryService(
    IRequestQueryService inner,
    HybridCache cache,
    IOptions<CacheOptions> cacheOptions) : IRequestQueryService
{
    private static readonly string[] SummaryTags = [RequestCacheKeys.RequestsTag];

    private readonly HybridCacheEntryOptions _summaryEntryOptions = new()
    {
        Expiration = cacheOptions.Value.SummaryExpiration,
        LocalCacheExpiration = cacheOptions.Value.SummaryLocalExpiration
    };

    public Task<PagedResult<RequestDto>> GetPagedAsync(
        RequestQueryParameters parameters,
        CancellationToken cancellationToken) =>
        inner.GetPagedAsync(parameters, cancellationToken);

    public Task<RequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        inner.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<RequestStatusHistoryDto>?> GetHistoryAsync(
        int requestId,
        CancellationToken cancellationToken) =>
        inner.GetHistoryAsync(requestId, cancellationToken);

    public Task<IReadOnlyList<string>> GetOrganizationNamesAsync(
        LookupQuery query,
        CancellationToken cancellationToken) =>
        inner.GetOrganizationNamesAsync(query, cancellationToken);

    public Task<IReadOnlyList<string>> GetAssigneesAsync(LookupQuery query, CancellationToken cancellationToken) =>
        inner.GetAssigneesAsync(query, cancellationToken);

    public async Task<RequestsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            RequestCacheKeys.Summary,
            async token => await inner.GetSummaryAsync(token),
            _summaryEntryOptions,
            SummaryTags,
            cancellationToken);
}
