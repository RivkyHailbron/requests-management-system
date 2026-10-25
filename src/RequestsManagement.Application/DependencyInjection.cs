using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests;
using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Application.Requests.Queries;

namespace RequestsManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddOptions<PaginationOptions>()
            .BindConfiguration(PaginationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<BulkUpdateOptions>()
            .BindConfiguration(BulkUpdateOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<LookupOptions>()
            .BindConfiguration(LookupOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;
        services.AddValidatorsFromAssemblyContaining<PaginationOptions>(ServiceLifetime.Singleton);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IRequestStatusService, RequestStatusService>();

        return services;
    }
}
