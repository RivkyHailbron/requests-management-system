using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RequestsManagement.Api;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;
using RequestsManagement.IntegrationTests.Infrastructure;

namespace RequestsManagement.IntegrationTests.Requests;

[Collection(ApiTestGroup.Name)]
public sealed class UpdateRequestStatusTests(RequestsApiFactory factory)
{
    private const string Agent = "agent.dana";
    private const string OtherAgent = "agent.omer";

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task UpdateStatus_WithAllowedTransition_UpdatesRequestAndRecordsHistory()
    {
        var request = await factory.CreateRequestAsync("Air quality complaint", "Status Org", RequestPriority.High);

        var response = await PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion, Agent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<RequestDto>(ApiJson.Options, CancellationToken);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.InProgress, updated.Status);
        Assert.NotEqual(RowVersionEncoding.Encode(request.RowVersion), updated.Version);
        Assert.True(updated.UpdatedAt > request.UpdatedAt);

        var history = await GetHistoryAsync(request.Id);
        var entry = Assert.Single(history);
        Assert.Equal(RequestStatus.New, entry.PreviousStatus);
        Assert.Equal(RequestStatus.InProgress, entry.NewStatus);
        Assert.Equal(Agent, entry.ChangedBy);
        Assert.Equal(updated.UpdatedAt, entry.ChangedAt);
    }

    [Fact]
    public async Task UpdateStatus_WithConcurrentUpdates_OnlyOneSucceedsAndOtherGetsConflict()
    {
        var request = await factory.CreateRequestAsync("Noise inspection request", "Race Org", RequestPriority.Medium);

        var responses = await Task.WhenAll(
            PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion, Agent),
            PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion, OtherAgent));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Single(await GetHistoryAsync(request.Id));
    }

    [Fact]
    public async Task UpdateStatus_WithStaleVersion_ReturnsConflictWithCurrentRequest()
    {
        var request = await factory.CreateRequestAsync("Waste disposal approval", "Stale Org", RequestPriority.Low);
        var firstResponse = await PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion, Agent);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var staleResponse = await PatchStatusAsync(request.Id, RequestStatus.InProgress, request.RowVersion, OtherAgent);

        Assert.Equal(HttpStatusCode.Conflict, staleResponse.StatusCode);
        var problem = await staleResponse.Content.ReadFromJsonAsync<ProblemDetails>(ApiJson.Options, CancellationToken);
        Assert.NotNull(problem);
        var currentRequest = ((JsonElement)problem.Extensions["currentRequest"]!).Deserialize<RequestDto>(ApiJson.Options);
        Assert.NotNull(currentRequest);
        Assert.Equal(RequestStatus.InProgress, currentRequest.Status);
        Assert.Single(await GetHistoryAsync(request.Id));
    }

    [Fact]
    public async Task UpdateStatus_WithDisallowedTransition_ReturnsUnprocessableEntity()
    {
        var request = await factory.CreateRequestAsync("Fuel storage inspection", "Transition Org", RequestPriority.Low);

        var response = await PatchStatusAsync(request.Id, RequestStatus.Completed, request.RowVersion, Agent);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Empty(await GetHistoryAsync(request.Id));
    }

    [Fact]
    public async Task UpdateStatus_WhenRequestDoesNotExist_ReturnsNotFound()
    {
        var response = await PatchStatusAsync(int.MaxValue, RequestStatus.InProgress, new byte[8], Agent);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidInput_ReturnsValidationProblem()
    {
        var request = await factory.CreateRequestAsync("Soil contamination report", "Invalid Org", RequestPriority.Low);
        var version = RowVersionEncoding.Encode(request.RowVersion);

        var unknownStatus = await SendStatusUpdateAsync(request.Id, new { status = "Archived", version }, Agent);
        var invalidVersion = await SendStatusUpdateAsync(request.Id, new { status = "InProgress", version = "not-a-version" }, Agent);
        var missingUser = await SendStatusUpdateAsync(request.Id, new { status = "InProgress", version }, userId: null);

        Assert.Equal(HttpStatusCode.BadRequest, unknownStatus.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidVersion.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingUser.StatusCode);
        Assert.Empty(await GetHistoryAsync(request.Id));
    }

    private Task<HttpResponseMessage> PatchStatusAsync(int requestId, RequestStatus status, byte[] version, string userId) =>
        SendStatusUpdateAsync(requestId, new { status, version = RowVersionEncoding.Encode(version) }, userId);

    private async Task<HttpResponseMessage> SendStatusUpdateAsync(int requestId, object body, string? userId)
    {
        using var message = new HttpRequestMessage(HttpMethod.Patch, new Uri($"/api/requests/{requestId}/status", UriKind.Relative))
        {
            Content = JsonContent.Create(body, options: ApiJson.Options)
        };

        if (userId is not null)
        {
            message.Headers.Add(ApiHeaders.UserId, userId);
        }

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
