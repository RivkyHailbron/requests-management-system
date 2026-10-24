using System.ComponentModel.DataAnnotations;

namespace RequestsManagement.Infrastructure.Caching;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan SummaryExpiration { get; init; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan SummaryLocalExpiration { get; init; }
}
