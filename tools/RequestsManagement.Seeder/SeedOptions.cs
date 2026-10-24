using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.Seeder;

internal sealed class SeedOptions
{
    public const string SectionName = "Seed";

    [Range(1, int.MaxValue)]
    public int RecordCount { get; init; }

    [Range(1, int.MaxValue)]
    public int BatchSize { get; init; }

    public int RandomSeed { get; init; }

    [Range(1, int.MaxValue)]
    public int HistoryDays { get; init; }

    [Range(1, int.MaxValue)]
    public int OrganizationCount { get; init; }

    [Range(1, int.MaxValue)]
    public int AssigneeCount { get; init; }

    public bool Reset { get; init; }
}
