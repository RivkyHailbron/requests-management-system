using Microsoft.Extensions.Caching.Hybrid;
using RequestsManagement.Application.Requests;

namespace RequestsManagement.Infrastructure.Caching;

internal sealed class RequestCacheInvalidator(HybridCache cache) : IRequestCacheInvalidator
{
    public async Task InvalidateAsync(CancellationToken cancellationToken) =>
        await cache.RemoveByTagAsync(RequestCacheKeys.RequestsTag, cancellationToken);
}
