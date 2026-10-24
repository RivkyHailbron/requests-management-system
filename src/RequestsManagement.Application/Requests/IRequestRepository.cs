using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Application.Requests;

public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken);

    void SetExpectedVersion(Request request, byte[] expectedVersion);

    void AddHistory(RequestStatusHistory historyEntry);

    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
