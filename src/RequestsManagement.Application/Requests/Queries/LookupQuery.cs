namespace RequestsManagement.Application.Requests.Queries;

public sealed class LookupQuery
{
    public string? Search { get; init; }

    public int? Limit { get; init; }
}
