namespace RequestsManagement.Application.Requests.Dtos;

public sealed record BulkStatusUpdateResultDto(
    int RequestedCount,
    int UpdatedCount,
    int FailedCount,
    IReadOnlyList<BulkStatusUpdateItemResultDto> Items);
