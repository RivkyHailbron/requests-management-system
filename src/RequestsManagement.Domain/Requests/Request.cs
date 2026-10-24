namespace RequestsManagement.Domain.Requests;

public sealed class Request
{
    private Request()
    {
    }

    public int Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string OrganizationName { get; private set; } = string.Empty;

    public RequestStatus Status { get; private set; }

    public RequestPriority Priority { get; private set; }

    public string? AssignedTo { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static Request Create(
        string title,
        string organizationName,
        RequestPriority priority,
        string? assignedTo,
        DateTime createdAt) =>
        new()
        {
            Title = title,
            OrganizationName = organizationName,
            Priority = priority,
            AssignedTo = assignedTo,
            Status = RequestStatus.New,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

    public bool CanChangeStatusTo(RequestStatus newStatus) =>
        RequestStatusTransitions.IsAllowed(Status, newStatus);

    public RequestStatusHistory ChangeStatus(RequestStatus newStatus, string changedBy, DateTime changedAt)
    {
        if (!CanChangeStatusTo(newStatus))
        {
            throw new InvalidStatusTransitionException(Status, newStatus);
        }

        var historyEntry = new RequestStatusHistory(Id, Status, newStatus, changedBy, changedAt);
        Status = newStatus;
        UpdatedAt = changedAt;
        return historyEntry;
    }
}
