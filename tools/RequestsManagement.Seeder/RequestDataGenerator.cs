using Bogus;
using Microsoft.Extensions.Options;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Seeder;

internal sealed class RequestDataGenerator
{
    private static readonly RequestStatus[] Statuses =
        [RequestStatus.New, RequestStatus.InProgress, RequestStatus.Waiting, RequestStatus.Completed];

    private static readonly float[] StatusWeights = [0.25f, 0.30f, 0.15f, 0.30f];

    private static readonly RequestPriority[] Priorities =
        [RequestPriority.Low, RequestPriority.Medium, RequestPriority.High];

    private static readonly float[] PriorityWeights = [0.40f, 0.40f, 0.20f];

    private static readonly string[] Topics =
    [
        "Business license renewal",
        "Hazardous materials permit",
        "Waste disposal approval",
        "Air quality complaint",
        "Noise inspection request",
        "Water discharge permit",
        "Emissions report submission",
        "Asbestos removal notification",
        "Recycling program enrollment",
        "Environmental impact assessment",
        "Soil contamination report",
        "Radiation source registration",
        "Packaging regulation inquiry",
        "Odor nuisance complaint",
        "Fuel storage inspection"
    ];

    private readonly SeedOptions _options;
    private readonly Faker _faker;
    private readonly string[] _organizations;
    private readonly string[] _assignees;
    private readonly DateTime _now;
    private readonly int _maxAgeInMinutes;

    public RequestDataGenerator(IOptions<SeedOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _faker = new Faker { Random = new Randomizer(_options.RandomSeed) };
        _organizations = CreateUniquePool(_options.OrganizationCount, () => _faker.Company.CompanyName());
        _assignees = CreateUniquePool(_options.AssigneeCount, () => _faker.Internet.UserName().ToLowerInvariant());
        _now = timeProvider.GetUtcNow().UtcDateTime;
        _maxAgeInMinutes = (int)TimeSpan.FromDays(_options.HistoryDays).TotalMinutes;
    }

    public IEnumerable<SeedRequest> Generate()
    {
        for (var index = 0; index < _options.RecordCount; index++)
        {
            yield return CreateRequest();
        }
    }

    private SeedRequest CreateRequest()
    {
        var status = _faker.Random.WeightedRandom(Statuses, StatusWeights);
        var organization = _faker.PickRandom(_organizations);
        var createdAt = _now.AddMinutes(-_faker.Random.Int(0, _maxAgeInMinutes));
        var updatedAt = status == RequestStatus.New
            ? createdAt
            : _faker.Date.Between(createdAt, _now);

        return new SeedRequest(
            Title: $"{_faker.PickRandom(Topics)} - {_faker.Address.City()}",
            OrganizationName: organization,
            Status: status,
            Priority: _faker.Random.WeightedRandom(Priorities, PriorityWeights),
            AssignedTo: status == RequestStatus.New ? null : _faker.PickRandom(_assignees),
            CreatedAt: createdAt,
            UpdatedAt: updatedAt);
    }

    private static string[] CreateUniquePool(int size, Func<string> factory)
    {
        var pool = new HashSet<string>(StringComparer.Ordinal);
        while (pool.Count < size)
        {
            pool.Add(factory());
        }

        return [.. pool];
    }
}
