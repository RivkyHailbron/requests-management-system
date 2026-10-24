using FluentValidation;
using RequestsManagement.Application.Common;

namespace RequestsManagement.Application.Requests.Commands;

public sealed class UpdateRequestStatusRequestValidator : AbstractValidator<UpdateRequestStatusRequest>
{
    public UpdateRequestStatusRequestValidator()
    {
        RuleFor(request => request.Status).IsInEnum();

        RuleFor(request => request.Version)
            .Must(version => RowVersionEncoding.TryDecode(version, out _))
            .WithMessage("'Version' must be the version value returned by the API.");
    }
}
