namespace RequestsManagement.Domain.Requests;

public sealed class InvalidStatusTransitionException(RequestStatus from, RequestStatus to)
    : InvalidOperationException($"Status transition from '{from}' to '{to}' is not allowed.")
{
    public RequestStatus From { get; } = from;

    public RequestStatus To { get; } = to;
}
