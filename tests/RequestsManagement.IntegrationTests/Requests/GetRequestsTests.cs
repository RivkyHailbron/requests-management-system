using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;
using RequestsManagement.IntegrationTests.Infrastructure;

namespace RequestsManagement.IntegrationTests.Requests;

[Collection(ApiTestGroup.Name)]
public sealed class GetRequestsTests(RequestsApiFactory factory)
{
    private const string RequestsUrl = "/api/requests";

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetRequests_WithCombinedFilters_ReturnsOnlyMatchingRequestsInRequestedOrder()
    {
        var organization = UniqueName("Alpha Industries");
        var high = await factory.CreateRequestAsync("Air quality complaint", organization, RequestPriority.High, RequestStatus.InProgress, "dana");
        var low = await factory.CreateRequestAsync("Noise inspection request", organization, RequestPriority.Low, RequestStatus.Completed, "dana");
        await factory.CreateRequestAsync("Waste disposal approval", organization, RequestPriority.High, RequestStatus.InProgress, "omer");
        await factory.CreateRequestAsync("Water discharge permit", organization, RequestPriority.Medium, RequestStatus.New);

        var result = await GetPageAsync(
            $"?organizationName={Uri.EscapeDataString(organization)}&status=InProgress&status=Completed&assignedTo=dana&sortBy=Priority&sortDirection=Desc");

        Assert.Equal(2, result.TotalCount);
        Assert.Equal([high.Id, low.Id], result.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetRequests_WithSearch_MatchesTitleAndOrganizationName()
    {
        var marker = UniqueName("Searchable");
        var byOrganization = await factory.CreateRequestAsync("Fuel storage inspection", marker, RequestPriority.Low);
        var byTitle = await factory.CreateRequestAsync($"{marker} permit", "Other Org", RequestPriority.Low);

        var result = await GetPageAsync($"?search={Uri.EscapeDataString(marker)}&sortBy=Title&sortDirection=Asc");

        Assert.Equal(2, result.TotalCount);
        Assert.Equal([byOrganization.Id, byTitle.Id], result.Items.Select(item => item.Id).Order());
    }

    [Fact]
    public async Task GetRequests_WithPaging_ReturnsRequestedPageAndTotals()
    {
        var organization = UniqueName("Paged Org");
        for (var index = 0; index < 5; index++)
        {
            await factory.CreateRequestAsync($"Request {index}", organization, RequestPriority.Medium);
        }

        var result = await GetPageAsync($"?organizationName={Uri.EscapeDataString(organization)}&page=3&pageSize=2");

        Assert.Single(result.Items);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, result.Page);
    }

    [Fact]
    public async Task GetRequests_WithInvalidParameters_ReturnsValidationProblem()
    {
        var response = await _client.GetAsync(
            new Uri($"{RequestsUrl}?page=0&pageSize=1000&createdFrom=2026-05-01&createdTo=2026-01-01", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ApiJson.Options, CancellationToken);
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("Page"));
        Assert.True(problem.Errors.ContainsKey("PageSize"));
        Assert.True(problem.Errors.ContainsKey("CreatedTo"));
    }

    [Fact]
    public async Task GetRequest_WhenRequestDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync(new Uri($"{RequestsUrl}/{int.MaxValue}", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsCountsForEveryStatusAndPriority()
    {
        await factory.CreateRequestAsync("Odor nuisance complaint", UniqueName("Summary Org"), RequestPriority.High, RequestStatus.Waiting);

        var summary = await _client.GetFromJsonAsync<RequestsSummaryDto>(
            new Uri($"{RequestsUrl}/summary", UriKind.Relative), ApiJson.Options, CancellationToken);

        Assert.NotNull(summary);
        Assert.Equal(Enum.GetValues<RequestStatus>(), summary.ByStatus.Select(entry => entry.Status));
        Assert.Equal(Enum.GetValues<RequestPriority>(), summary.ByPriority.Select(entry => entry.Priority));
        Assert.Equal(summary.TotalCount, summary.ByStatus.Sum(entry => entry.Count));
        Assert.Equal(summary.TotalCount, summary.ByPriority.Sum(entry => entry.Count));
        Assert.True(summary.ByStatus.Single(entry => entry.Status == RequestStatus.Waiting).Count > 0);
    }

    private async Task<PagedResult<RequestDto>> GetPageAsync(string queryString)
    {
        var result = await _client.GetFromJsonAsync<PagedResult<RequestDto>>(
            new Uri(RequestsUrl + queryString, UriKind.Relative), ApiJson.Options, CancellationToken);
        Assert.NotNull(result);
        return result;
    }

    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}";
}
