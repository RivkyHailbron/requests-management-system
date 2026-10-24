using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Seeder;

internal sealed record SeedRequest(
    string Title,
    string OrganizationName,
    RequestStatus Status,
    RequestPriority Priority,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime UpdatedAt);
