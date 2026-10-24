using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Dtos;

public sealed record PriorityCountDto(RequestPriority Priority, int Count);
