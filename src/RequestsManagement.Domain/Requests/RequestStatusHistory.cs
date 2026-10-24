namespace RequestsManagement.Domain.Requests;

public sealed class RequestStatusHistory
{
    private RequestStatusHistory()
    {
    }

    internal RequestStatusHistory(
        int requestId,
        RequestStatus previousStatus,
        RequestStatus newStatus,
        string changedBy,
        DateTime changedAt)
    {
        RequestId = requestId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
    }

    public long Id { get; private set; }

    public int RequestId { get; private set; }

    public RequestStatus PreviousStatus { get; private set; }

    public RequestStatus NewStatus { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public string ChangedBy { get; private set; } = string.Empty;
}
