using System.Net;
using System.Net.Http.Json;
using RequestsManagement.Api;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;
using RequestsManagement.IntegrationTests.Infrastructure;

namespace RequestsManagement.IntegrationTests.Requests;

[Collection(ApiTestGroup.Name)]
public sealed class BulkUpdateRequestStatusTests(RequestsApiFactory factory)
{
    private const string Agent = "agent.dana";
    private const int MaxBulkItems = 100;

    private static readonly Uri BulkStatusUrl = new("/api/requests/status", UriKind.Relative);

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task BulkUpdate_WithMixedItems_UpdatesValidItemsAndReportsEachFailure()
    {
        var valid = await factory.CreateRequestAsync("Air quality complaint", "Bulk Org", RequestPriority.High);
        var stale = await factory.CreateRequestAsync("Noise inspection request", "Bulk Org", RequestPriority.Low);
        var completed = await factory.CreateRequestAsync("Water discharge permit", "Bulk Org", RequestPriority.Medium, RequestStatus.Completed);
        var missingId = int.MaxValue;

        var response = await SendBulkUpdateAsync(new
        {
            status = RequestStatus.InProgress,
            items = new[]
            {
                new { id = valid.Id, version = RowVersionEncoding.Encode(valid.RowVersion) },
                new { id = stale.Id, version = RowVersionEncoding.Encode(new byte[8]) },
                new { id = completed.Id, version = RowVersionEncoding.Encode(completed.RowVersion) },
                new { id = missingId, version = RowVersionEncoding.Encode(new byte[8]) }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkStatusUpdateResultDto>(ApiJson.Options, CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(4, result.RequestedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(3, result.FailedCount);

        var outcomes = result.Items.ToDictionary(item => item.Id, item => item.Outcome);
        Assert.Equal(StatusUpdateOutcome.Updated, outcomes[valid.Id]);
        Assert.Equal(StatusUpdateOutcome.Conflict, outcomes[stale.Id]);
        Assert.Equal(StatusUpdateOutcome.InvalidTransition, outcomes[completed.Id]);
        Assert.Equal(StatusUpdateOutcome.NotFound, outcomes[missingId]);

        Assert.Single(await GetHistoryAsync(valid.Id));
        Assert.Empty(await GetHistoryAsync(stale.Id));
    }

    [Fact]
    public async Task BulkUpdate_WithTooManyOrDuplicateItems_ReturnsValidationProblem()
    {
        var version = RowVersionEncoding.Encode(new byte[8]);

        var tooMany = await SendBulkUpdateAsync(new
        {
            status = RequestStatus.InProgress,
            items = Enumerable.Range(1, MaxBulkItems + 1).Select(id => new { id, version })
        });
        var duplicates = await SendBulkUpdateAsync(new
        {
            status = RequestStatus.InProgress,
            items = new[] { new { id = 1, version }, new { id = 1, version } }
        });
        var empty = await SendBulkUpdateAsync(new { status = RequestStatus.InProgress, items = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicates.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
    }

    private async Task<HttpResponseMessage> SendBulkUpdateAsync(object body)
    {
        using var message = new HttpRequestMessage(HttpMethod.Patch, BulkStatusUrl)
        {
            Content = JsonContent.Create(body, options: ApiJson.Options)
        };
        message.Headers.Add(ApiHeaders.UserId, Agent);
        return await _client.SendAsync(message, CancellationToken);
    }

    private async Task<IReadOnlyList<RequestStatusHistoryDto>> GetHistoryAsync(int requestId)
    {
        var history = await _client.GetFromJsonAsync<IReadOnlyList<RequestStatusHistoryDto>>(
            new Uri($"/api/requests/{requestId}/history", UriKind.Relative), ApiJson.Options, CancellationToken);
        Assert.NotNull(history);
        return history;
    }
}
