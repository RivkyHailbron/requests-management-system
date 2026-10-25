using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.Application.Requests.Queries;

public sealed class LookupOptions
{
    public const string SectionName = "Lookup";

    [Range(1, int.MaxValue)]
    public int DefaultLimit { get; init; }

    [Range(1, int.MaxValue)]
    public int MaxLimit { get; init; }
}
