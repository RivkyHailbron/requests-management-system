using FluentValidation;
using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;

namespace RequestsManagement.Application.Requests.Commands;

public sealed class BulkUpdateRequestStatusRequestValidator : AbstractValidator<BulkUpdateRequestStatusRequest>
{
    public BulkUpdateRequestStatusRequestValidator(IOptions<BulkUpdateOptions> bulkUpdateOptions)
    {
        var maxItems = bulkUpdateOptions.Value.MaxItems;

        RuleFor(request => request.Status).IsInEnum();

        RuleFor(request => request.Items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(items => items.Count <= maxItems)
            .WithMessage($"'Items' must contain at most {maxItems} requests.")
            .Must(items => items.Select(item => item.Id).Distinct().Count() == items.Count)
            .WithMessage("'Items' must not contain the same request more than once.");

        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(entry => entry.Id).GreaterThan(0);

            item.RuleFor(entry => entry.Version)
                .Must(version => RowVersionEncoding.TryDecode(version, out _))
                .WithMessage("'Version' must be the version value returned by the API.");
        });
    }
}
