using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Requests;
using RequestsManagement.Infrastructure.Caching;
using RequestsManagement.Infrastructure.Persistence;
using RequestsManagement.Infrastructure.Requests;

namespace RequestsManagement.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "RequestsDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<RequestsDbContext>(options => options.UseSqlServer(connectionString));

        services
            .AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHybridCache();

        services.AddScoped<RequestQueryService>();
        services.AddScoped<IRequestQueryService>(provider => new CachedRequestQueryService(
            provider.GetRequiredService<RequestQueryService>(),
            provider.GetRequiredService<HybridCache>(),
            provider.GetRequiredService<IOptions<CacheOptions>>()));
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddSingleton<IRequestCacheInvalidator, RequestCacheInvalidator>();

        return services;
    }
}
