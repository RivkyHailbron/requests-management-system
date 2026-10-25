using FluentValidation;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;

namespace RequestsManagement.Application.Requests.Queries;

public sealed class LookupQueryValidator : AbstractValidator<LookupQuery>
{
    public LookupQueryValidator(IOptions<LookupOptions> lookupOptions, IOptions<PaginationOptions> paginationOptions)
    {
        RuleFor(query => query.Search)
            .MaximumLength(paginationOptions.Value.MaxSearchLength);

        RuleFor(query => query.Limit)
            .InclusiveBetween(1, lookupOptions.Value.MaxLimit)
            .When(query => query.Limit.HasValue);
    }
}
