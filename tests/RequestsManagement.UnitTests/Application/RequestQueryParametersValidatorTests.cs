using Microsoft.Extensions.Options;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Queries;

namespace RequestsManagement.UnitTests.Application;

public sealed class RequestQueryParametersValidatorTests
{
    private const int MaxPageSize = 100;
    private const int MaxSearchLength = 50;

    private readonly RequestQueryParametersValidator _validator = new(Options.Create(new PaginationOptions
    {
        DefaultPageSize = 20,
        MaxPageSize = MaxPageSize,
        MaxSearchLength = MaxSearchLength
    }));

    [Fact]
    public void Validate_WithDefaults_IsValid() =>
        Assert.True(_validator.Validate(new RequestQueryParameters()).IsValid);

    [Theory]
    [InlineData(0)]
    [InlineData(MaxPageSize + 1)]
    public void Validate_WithPageSizeOutOfRange_IsInvalid(int pageSize)
    {
        var result = _validator.Validate(new RequestQueryParameters { PageSize = pageSize });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RequestQueryParameters.PageSize));
    }

    [Fact]
    public void Validate_WithCreatedToBeforeCreatedFrom_IsInvalid()
    {
        var createdFrom = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _validator.Validate(new RequestQueryParameters
        {
            CreatedFrom = createdFrom,
            CreatedTo = createdFrom.AddDays(-1)
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RequestQueryParameters.CreatedTo));
    }

    [Fact]
    public void Validate_WithSearchLongerThanLimit_IsInvalid()
    {
        var result = _validator.Validate(new RequestQueryParameters { Search = new string('a', MaxSearchLength + 1) });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RequestQueryParameters.Search));
    }
}
