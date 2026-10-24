using System.Net;
using System.Net.Http.Json;
using RequestsManagement.Api;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;
using RequestsManagement.IntegrationTests.Infrastructure;

namespace RequestsManagement.IntegrationTests.Requests;

[Collection(ApiTestGroup.Name)]
public sealed class SummaryCacheTests(RequestsApiFactory factory)
{
    private static readonly Uri SummaryUrl = new("/api/requests/summary", UriKind.Relative);

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetSummary_AfterStatusUpdate_ReturnsFreshCountsInsteadOfCachedValue()
    {
        var request = await factory.CreateRequestAsync("Recycling program enrollment", "Cache Org", RequestPriority.Low);
        var before = await GetSummaryAsync();

        using var message = new HttpRequestMessage(HttpMethod.Patch, new Uri($"/api/requests/{request.Id}/status", UriKind.Relative))
        {
            Content = JsonContent.Create(
                new { status = RequestStatus.InProgress, version = RowVersionEncoding.Encode(request.RowVersion) },
                options: ApiJson.Options)
        };
        message.Headers.Add(ApiHeaders.UserId, "agent.cache");
        var updateResponse = await _client.SendAsync(message, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var after = await GetSummaryAsync();

        Assert.Equal(CountOf(before, RequestStatus.New) - 1, CountOf(after, RequestStatus.New));
        Assert.Equal(CountOf(before, RequestStatus.InProgress) + 1, CountOf(after, RequestStatus.InProgress));
        Assert.Equal(before.TotalCount, after.TotalCount);
    }

    private async Task<RequestsSummaryDto> GetSummaryAsync()
    {
        var summary = await _client.GetFromJsonAsync<RequestsSummaryDto>(SummaryUrl, ApiJson.Options, CancellationToken);
        Assert.NotNull(summary);
        return summary;
    }

    private static int CountOf(RequestsSummaryDto summary, RequestStatus status) =>
        summary.ByStatus.Single(entry => entry.Status == status).Count;
}
