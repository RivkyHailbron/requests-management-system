using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RequestsManagement.Application.Common;

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

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;
        services.AddValidatorsFromAssemblyContaining<PaginationOptions>(ServiceLifetime.Singleton);

        return services;
    }
}
