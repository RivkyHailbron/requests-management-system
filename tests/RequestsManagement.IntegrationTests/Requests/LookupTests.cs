using System.Net;
using System.Net.Http.Json;
using RequestsManagement.Domain.Requests;
using RequestsManagement.IntegrationTests.Infrastructure;

namespace RequestsManagement.IntegrationTests.Requests;

[Collection(ApiTestGroup.Name)]
public sealed class LookupTests(RequestsApiFactory factory)
{
    private const string OrganizationsUrl = "/api/requests/lookups/organizations";
    private const string AssigneesUrl = "/api/requests/lookups/assignees";
    private const int MaxLimit = 50;

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetOrganizationNames_WithSearch_ReturnsDistinctNamesStartingWithSearchInOrder()
    {
        var marker = Guid.NewGuid().ToString("N");
        var beta = $"{marker} Beta Ltd";
        var alpha = $"{marker} Alpha Ltd";
        await factory.CreateRequestAsync("Air quality complaint", beta, RequestPriority.Low);
        await factory.CreateRequestAsync("Noise inspection request", alpha, RequestPriority.High);
        await factory.CreateRequestAsync("Waste disposal approval", alpha, RequestPriority.Medium);
        await factory.CreateRequestAsync("Odor nuisance complaint", $"Gamma {marker} Ltd", RequestPriority.Low);

        var names = await GetValuesAsync($"{OrganizationsUrl}?search={marker}");

        Assert.Equal([alpha, beta], names);
    }

    [Fact]
    public async Task GetAssignees_ReturnsOnlyAssignedValuesAndRespectsLimit()
    {
        var marker = Guid.NewGuid().ToString("N");
        var first = $"{marker}.agent.a";
        var second = $"{marker}.agent.b";
        await factory.CreateRequestAsync("Fuel storage inspection", "Lookup Org", RequestPriority.Low, RequestStatus.InProgress, second);
        await factory.CreateRequestAsync("Soil contamination report", "Lookup Org", RequestPriority.Low, RequestStatus.InProgress, first);
        await factory.CreateRequestAsync("Odor nuisance complaint", "Lookup Org", RequestPriority.Low);

        var all = await GetValuesAsync($"{AssigneesUrl}?search={marker}");
        var limited = await GetValuesAsync($"{AssigneesUrl}?search={marker}&limit=1");

        Assert.Equal([first, second], all);
        Assert.Equal([first], limited);
    }

    [Fact]
    public async Task GetLookups_WithInvalidLimit_ReturnsValidationProblem()
    {
        var tooLarge = await _client.GetAsync(new Uri($"{OrganizationsUrl}?limit={MaxLimit + 1}", UriKind.Relative), CancellationToken);
        var zero = await _client.GetAsync(new Uri($"{AssigneesUrl}?limit=0", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, tooLarge.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);
    }

    private async Task<IReadOnlyList<string>> GetValuesAsync(string url)
    {
        var values = await _client.GetFromJsonAsync<IReadOnlyList<string>>(new Uri(url, UriKind.Relative), ApiJson.Options, CancellationToken);
        Assert.NotNull(values);
        return values;
    }
}
