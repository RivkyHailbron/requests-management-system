using RequestsManagement.Application.Common;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Queries;

public sealed class RequestQueryParameters
{
    public int Page { get; init; } = 1;

    public int? PageSize { get; init; }

    public IReadOnlyList<RequestStatus>? Status { get; init; }

    public IReadOnlyList<RequestPriority>? Priority { get; init; }

    public string? OrganizationName { get; init; }

    public string? AssignedTo { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }

    public string? Search { get; init; }

    public RequestSortField SortBy { get; init; } = RequestSortField.CreatedAt;

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
