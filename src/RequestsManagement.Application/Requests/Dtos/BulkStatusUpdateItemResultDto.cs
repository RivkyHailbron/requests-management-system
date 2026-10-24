using RequestsManagement.Application.Requests.Commands;

namespace RequestsManagement.Application.Requests.Dtos;

public sealed record BulkStatusUpdateItemResultDto(int Id, StatusUpdateOutcome Outcome, RequestDto? Request);
