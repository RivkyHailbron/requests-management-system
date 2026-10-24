using RequestsManagement.Domain.Requests;

namespace RequestsManagement.IntegrationTests.Infrastructure;

internal static class TestStatusPaths
{
    public static IReadOnlyList<RequestStatus> To(RequestStatus status) => status switch
    {
        RequestStatus.New => [],
        RequestStatus.InProgress => [RequestStatus.InProgress],
        RequestStatus.Waiting => [RequestStatus.InProgress, RequestStatus.Waiting],
        RequestStatus.Completed => [RequestStatus.InProgress, RequestStatus.Completed],
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
