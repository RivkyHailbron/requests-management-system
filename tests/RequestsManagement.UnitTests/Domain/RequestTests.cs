using RequestsManagement.Domain.Requests;

namespace RequestsManagement.UnitTests.Domain;

public sealed class RequestTests
{
    private const string ChangedBy = "agent.smith";

    private static readonly DateTime CreatedAt = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_InitializesNewRequestWithMatchingTimestamps()
    {
        var request = CreateRequest();

        Assert.Equal(RequestStatus.New, request.Status);
        Assert.Equal(CreatedAt, request.CreatedAt);
        Assert.Equal(CreatedAt, request.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_WithAllowedTransition_UpdatesStatusAndReturnsHistoryEntry()
    {
        var request = CreateRequest();
        var changedAt = CreatedAt.AddHours(1);

        var historyEntry = request.ChangeStatus(RequestStatus.InProgress, ChangedBy, changedAt);

        Assert.Equal(RequestStatus.InProgress, request.Status);
        Assert.Equal(changedAt, request.UpdatedAt);
        Assert.Equal(RequestStatus.New, historyEntry.PreviousStatus);
        Assert.Equal(RequestStatus.InProgress, historyEntry.NewStatus);
        Assert.Equal(ChangedBy, historyEntry.ChangedBy);
        Assert.Equal(changedAt, historyEntry.ChangedAt);
    }

    [Fact]
    public void ChangeStatus_WithDisallowedTransition_ThrowsAndKeepsState()
    {
        var request = CreateRequest();

        var exception = Assert.Throws<InvalidStatusTransitionException>(
            () => request.ChangeStatus(RequestStatus.Completed, ChangedBy, CreatedAt.AddHours(1)));

        Assert.Equal(RequestStatus.New, exception.From);
        Assert.Equal(RequestStatus.Completed, exception.To);
        Assert.Equal(RequestStatus.New, request.Status);
        Assert.Equal(CreatedAt, request.UpdatedAt);
    }

    private static Request CreateRequest() =>
        Request.Create("Noise inspection request", "Acme Ltd", RequestPriority.Medium, assignedTo: null, CreatedAt);
}
