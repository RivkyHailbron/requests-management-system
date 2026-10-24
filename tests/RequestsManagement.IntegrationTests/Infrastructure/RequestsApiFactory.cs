using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RequestsManagement.Domain.Requests;
using RequestsManagement.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace RequestsManagement.IntegrationTests.Infrastructure;

public sealed class RequestsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string TestEnvironment = "Testing";
    private const string ConnectionStringKey = "ConnectionStrings:RequestsDb";

    private readonly MsSqlContainer _database = new MsSqlBuilder(SqlServerImage).Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task<Request> CreateRequestAsync(
        string title,
        string organizationName,
        RequestPriority priority,
        RequestStatus status = RequestStatus.New,
        string? assignedTo = null)
    {
        var createdAt = DateTime.UtcNow;
        var request = Request.Create(title, organizationName, priority, assignedTo, createdAt);
        foreach (var nextStatus in TestStatusPaths.To(status))
        {
            request.ChangeStatus(nextStatus, TestUsers.Seeder, createdAt);
        }

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestsDbContext>();
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();
        return request;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(TestEnvironment);
        builder.UseSetting(ConnectionStringKey, _database.GetConnectionString());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
