using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests.Commands;

public sealed record BulkUpdateRequestStatusRequest(RequestStatus Status, IReadOnlyList<BulkStatusUpdateItem> Items);
