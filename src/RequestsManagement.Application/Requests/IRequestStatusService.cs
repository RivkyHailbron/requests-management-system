using RequestsManagement.Application.Requests.Commands;

namespace RequestsManagement.Application.Requests;

public interface IRequestStatusService
{
    Task<StatusUpdateResult> UpdateStatusAsync(
        int requestId,
        UpdateRequestStatusRequest command,
        string changedBy,
        CancellationToken cancellationToken);
}
