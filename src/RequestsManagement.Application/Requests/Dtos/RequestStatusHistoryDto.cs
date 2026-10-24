using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Dtos;

public sealed record RequestStatusHistoryDto(
    long Id,
    int RequestId,
    RequestStatus PreviousStatus,
    RequestStatus NewStatus,
    DateTime ChangedAt,
    string ChangedBy);
