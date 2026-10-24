using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RequestsManagement.Application.Requests;
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

        services.AddScoped<IRequestQueryService, RequestQueryService>();

        return services;
    }
}
