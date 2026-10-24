using FluentValidation;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Queries;

public sealed class RequestQueryParametersValidator : AbstractValidator<RequestQueryParameters>
{
    public RequestQueryParametersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var pagination = paginationOptions.Value;

        RuleFor(parameters => parameters.Page).GreaterThanOrEqualTo(1);

        RuleFor(parameters => parameters.PageSize)
            .InclusiveBetween(1, pagination.MaxPageSize)
            .When(parameters => parameters.PageSize.HasValue);

        RuleForEach(parameters => parameters.Status).IsInEnum();

        RuleForEach(parameters => parameters.Priority).IsInEnum();

        RuleFor(parameters => parameters.OrganizationName)
            .MaximumLength(RequestConstraints.OrganizationNameMaxLength);

        RuleFor(parameters => parameters.AssignedTo)
            .MaximumLength(RequestConstraints.UserIdentifierMaxLength);

        RuleFor(parameters => parameters.Search)
            .MaximumLength(pagination.MaxSearchLength);

        RuleFor(parameters => parameters.CreatedTo)
            .GreaterThanOrEqualTo(parameters => parameters.CreatedFrom)
            .When(parameters => parameters.CreatedFrom.HasValue && parameters.CreatedTo.HasValue)
            .WithMessage("'Created To' must be later than or equal to 'Created From'.");

        RuleFor(parameters => parameters.SortBy).IsInEnum();

        RuleFor(parameters => parameters.SortDirection).IsInEnum();
    }
}
