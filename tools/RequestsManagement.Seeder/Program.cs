using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RequestsManagement.Infrastructure;
using RequestsManagement.Seeder;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services
    .AddOptions<SeedOptions>()
    .Bind(builder.Configuration.GetSection(SeedOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<RequestDataGenerator>();
builder.Services.AddScoped<DatabaseSeeder>();

using var host = builder.Build();
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

await using var scope = host.Services.CreateAsyncScope();
await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync(cancellation.Token);
