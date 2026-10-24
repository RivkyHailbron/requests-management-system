using RequestsManagement.Application.Requests.Dtos;

namespace RequestsManagement.Application.Requests.Commands;

public sealed record StatusUpdateResult(StatusUpdateOutcome Outcome, RequestDto? Request)
{
    public static StatusUpdateResult Updated(RequestDto request) => new(StatusUpdateOutcome.Updated, request);

    public static StatusUpdateResult NotFound() => new(StatusUpdateOutcome.NotFound, null);

    public static StatusUpdateResult Conflict(RequestDto? currentRequest) =>
        currentRequest is null ? NotFound() : new(StatusUpdateOutcome.Conflict, currentRequest);

    public static StatusUpdateResult InvalidTransition(RequestDto currentRequest) =>
        new(StatusUpdateOutcome.InvalidTransition, currentRequest);
}
