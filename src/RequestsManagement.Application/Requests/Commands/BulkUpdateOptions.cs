using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.Application.Requests.Commands;

public sealed class BulkUpdateOptions
{
    public const string SectionName = "BulkUpdate";

    [Range(1, int.MaxValue)]
    public int MaxItems { get; init; }
}
