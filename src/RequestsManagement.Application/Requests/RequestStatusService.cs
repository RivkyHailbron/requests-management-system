using Microsoft.Extensions.Logging;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Application.Requests.Dtos;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests;

internal sealed partial class RequestStatusService(
    IRequestRepository repository,
    IRequestQueryService queryService,
    IRequestCacheInvalidator cacheInvalidator,
    TimeProvider timeProvider,
    ILogger<RequestStatusService> logger) : IRequestStatusService
{
    public async Task<StatusUpdateResult> UpdateStatusAsync(
        int requestId,
        UpdateRequestStatusRequest command,
        string changedBy,
        CancellationToken cancellationToken)
    {
        var request = await repository.GetByIdAsync(requestId, cancellationToken);
        var result = await ApplyStatusChangeAsync(
            requestId,
            request,
            command.Status,
            DecodeVersion(command.Version),
            changedBy,
            cancellationToken);

        if (result.Outcome == StatusUpdateOutcome.Updated)
        {
            await cacheInvalidator.InvalidateAsync(CancellationToken.None);
        }

        return result;
    }

    public async Task<BulkStatusUpdateResultDto> BulkUpdateStatusAsync(
        BulkUpdateRequestStatusRequest command,
        string changedBy,
        CancellationToken cancellationToken)
    {
        var requests = await repository.GetByIdsAsync(
            [.. command.Items.Select(item => item.Id)],
            cancellationToken);

        var results = new List<BulkStatusUpdateItemResultDto>(command.Items.Count);
        foreach (var item in command.Items)
        {
            var result = await ApplyStatusChangeAsync(
                item.Id,
                requests.GetValueOrDefault(item.Id),
                command.Status,
                DecodeVersion(item.Version),
                changedBy,
                cancellationToken);

            results.Add(new BulkStatusUpdateItemResultDto(item.Id, result.Outcome, result.Request));
        }

        var updatedCount = results.Count(result => result.Outcome == StatusUpdateOutcome.Updated);
        if (updatedCount > 0)
        {
            await cacheInvalidator.InvalidateAsync(CancellationToken.None);
        }

        LogBulkUpdateCompleted(logger, command.Items.Count, updatedCount, command.Status);

        return new BulkStatusUpdateResultDto(
            command.Items.Count,
            updatedCount,
            command.Items.Count - updatedCount,
            results);
    }

    private async Task<StatusUpdateResult> ApplyStatusChangeAsync(
        int requestId,
        Request? request,
        RequestStatus newStatus,
        byte[] expectedVersion,
        string changedBy,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return StatusUpdateResult.NotFound();
        }

        if (!request.RowVersion.AsSpan().SequenceEqual(expectedVersion))
        {
            LogConcurrencyConflict(logger, requestId);
            return StatusUpdateResult.Conflict(request.ToDto());
        }

        if (!request.CanChangeStatusTo(newStatus))
        {
            return StatusUpdateResult.InvalidTransition(request.ToDto());
        }

        var previousStatus = request.Status;
        repository.SetExpectedVersion(request, expectedVersion);
        repository.AddHistory(request.ChangeStatus(newStatus, changedBy, timeProvider.GetUtcNow().UtcDateTime));

        if (!await repository.TrySaveChangesAsync(cancellationToken))
        {
            LogConcurrencyConflict(logger, requestId);
            return StatusUpdateResult.Conflict(await queryService.GetByIdAsync(requestId, cancellationToken));
        }

        LogStatusChanged(logger, requestId, previousStatus, newStatus);
        return StatusUpdateResult.Updated(request.ToDto());
    }

    private static byte[] DecodeVersion(string version) =>
        RowVersionEncoding.TryDecode(version, out var rowVersion)
            ? rowVersion
            : throw new ArgumentException("Version must be a valid encoded row version.", nameof(version));

    [LoggerMessage(Level = LogLevel.Information, Message = "Request {RequestId} status changed from {PreviousStatus} to {NewStatus}")]
    private static partial void LogStatusChanged(ILogger logger, int requestId, RequestStatus previousStatus, RequestStatus newStatus);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Concurrency conflict while updating request {RequestId}")]
    private static partial void LogConcurrencyConflict(ILogger logger, int requestId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Bulk status update to {NewStatus} completed: {UpdatedCount}/{RequestedCount} updated")]
    private static partial void LogBulkUpdateCompleted(ILogger logger, int requestedCount, int updatedCount, RequestStatus newStatus);
}
