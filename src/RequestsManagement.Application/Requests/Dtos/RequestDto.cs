using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Dtos;

public sealed record RequestDto(
    int Id,
    string Title,
    string OrganizationName,
    RequestStatus Status,
    RequestPriority Priority,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Version);
