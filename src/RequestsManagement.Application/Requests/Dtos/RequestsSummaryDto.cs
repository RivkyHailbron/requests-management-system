namespace RequestsManagement.Application.Requests.Dtos;

public sealed record RequestsSummaryDto(
    int TotalCount,
    IReadOnlyList<StatusCountDto> ByStatus,
    IReadOnlyList<PriorityCountDto> ByPriority);
