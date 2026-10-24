using RequestsManagement.Domain.Requests;

namespace RequestsManagement.UnitTests.Domain;

public sealed class RequestStatusTransitionsTests
{
    [Theory]
    [InlineData(RequestStatus.New, RequestStatus.InProgress)]
    [InlineData(RequestStatus.InProgress, RequestStatus.Waiting)]
    [InlineData(RequestStatus.InProgress, RequestStatus.Completed)]
    [InlineData(RequestStatus.Waiting, RequestStatus.InProgress)]
    [InlineData(RequestStatus.Waiting, RequestStatus.Completed)]
    public void IsAllowed_ForDefinedTransition_ReturnsTrue(RequestStatus from, RequestStatus to) =>
        Assert.True(RequestStatusTransitions.IsAllowed(from, to));

    [Theory]
    [InlineData(RequestStatus.New, RequestStatus.New)]
    [InlineData(RequestStatus.New, RequestStatus.Waiting)]
    [InlineData(RequestStatus.New, RequestStatus.Completed)]
    [InlineData(RequestStatus.Completed, RequestStatus.New)]
    [InlineData(RequestStatus.Completed, RequestStatus.InProgress)]
    [InlineData(RequestStatus.Waiting, RequestStatus.New)]
    public void IsAllowed_ForUndefinedTransition_ReturnsFalse(RequestStatus from, RequestStatus to) =>
        Assert.False(RequestStatusTransitions.IsAllowed(from, to));

    [Fact]
    public void GetAllowedTargets_ForCompleted_ReturnsEmpty() =>
        Assert.Empty(RequestStatusTransitions.GetAllowedTargets(RequestStatus.Completed));
}
