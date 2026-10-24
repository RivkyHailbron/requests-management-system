using System.Collections.Frozen;

namespace RequestsManagement.Domain.Requests;

public static class RequestStatusTransitions
{
    private static readonly FrozenDictionary<RequestStatus, FrozenSet<RequestStatus>> AllowedTransitions =
        new Dictionary<RequestStatus, FrozenSet<RequestStatus>>
        {
            [RequestStatus.New] = FrozenSet.Create(RequestStatus.InProgress),
            [RequestStatus.InProgress] = FrozenSet.Create(RequestStatus.Waiting, RequestStatus.Completed),
            [RequestStatus.Waiting] = FrozenSet.Create(RequestStatus.InProgress, RequestStatus.Completed),
            [RequestStatus.Completed] = FrozenSet<RequestStatus>.Empty
        }.ToFrozenDictionary();

    public static bool IsAllowed(RequestStatus from, RequestStatus to) =>
        GetAllowedTargets(from).Contains(to);

    public static IReadOnlySet<RequestStatus> GetAllowedTargets(RequestStatus from) =>
        AllowedTransitions.GetValueOrDefault(from, FrozenSet<RequestStatus>.Empty);
}
