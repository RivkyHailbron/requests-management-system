using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Dtos;

public sealed record StatusCountDto(RequestStatus Status, int Count);
