using Microsoft.EntityFrameworkCore;
using RequestsManagement.Application.Requests;
using RequestsManagement.Domain.Requests;
using RequestsManagement.Infrastructure.Persistence;

namespace RequestsManagement.Infrastructure.Requests;

internal sealed class RequestRepository(RequestsDbContext dbContext) : IRequestRepository
{
    public Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Requests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    public void SetExpectedVersion(Request request, byte[] expectedVersion) =>
        dbContext.Entry(request).Property(entity => entity.RowVersion).OriginalValue = expectedVersion;

    public void AddHistory(RequestStatusHistory historyEntry) =>
        dbContext.RequestStatusHistory.Add(historyEntry);

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }
}
