using Microsoft.Extensions.Logging;
using RequestsManagement.Application.Common;
using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests;

internal sealed partial class RequestStatusService(
    IRequestRepository repository,
    IRequestQueryService queryService,
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
        if (request is null)
        {
            return StatusUpdateResult.NotFound();
        }

        if (!RowVersionEncoding.TryDecode(command.Version, out var expectedVersion))
        {
            throw new ArgumentException("Version must be a valid encoded row version.", nameof(command));
        }

        if (!request.RowVersion.AsSpan().SequenceEqual(expectedVersion))
        {
            LogConcurrencyConflict(logger, requestId);
            return StatusUpdateResult.Conflict(request.ToDto());
        }

        if (!request.CanChangeStatusTo(command.Status))
        {
            return StatusUpdateResult.InvalidTransition(request.ToDto());
        }

        var previousStatus = request.Status;
        repository.SetExpectedVersion(request, expectedVersion);
        repository.AddHistory(request.ChangeStatus(command.Status, changedBy, timeProvider.GetUtcNow().UtcDateTime));

        if (!await repository.TrySaveChangesAsync(cancellationToken))
        {
            LogConcurrencyConflict(logger, requestId);
            return StatusUpdateResult.Conflict(await queryService.GetByIdAsync(requestId, cancellationToken));
        }

        LogStatusChanged(logger, requestId, previousStatus, command.Status);
        return StatusUpdateResult.Updated(request.ToDto());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Request {RequestId} status changed from {PreviousStatus} to {NewStatus}")]
    private static partial void LogStatusChanged(ILogger logger, int requestId, RequestStatus previousStatus, RequestStatus newStatus);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Concurrency conflict while updating request {RequestId}")]
    private static partial void LogConcurrencyConflict(ILogger logger, int requestId);
}
