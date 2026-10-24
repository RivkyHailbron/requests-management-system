using RequestsManagement.Application.Requests.Commands;
using RequestsManagement.Application.Requests.Dtos;

namespace RequestsManagement.Application.Requests;

public interface IRequestStatusService
{
    Task<StatusUpdateResult> UpdateStatusAsync(
        int requestId,
        UpdateRequestStatusRequest command,
        string changedBy,
        CancellationToken cancellationToken);

    Task<BulkStatusUpdateResultDto> BulkUpdateStatusAsync(
        BulkUpdateRequestStatusRequest command,
        string changedBy,
        CancellationToken cancellationToken);
}
