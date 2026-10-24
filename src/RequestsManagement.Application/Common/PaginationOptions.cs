using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.Application.Common;

public sealed class PaginationOptions
{
    public const string SectionName = "Pagination";

    [Range(1, int.MaxValue)]
    public int DefaultPageSize { get; init; }

    [Range(1, int.MaxValue)]
    public int MaxPageSize { get; init; }

    [Range(1, int.MaxValue)]
    public int MaxSearchLength { get; init; }
}
