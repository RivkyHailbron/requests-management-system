using Microsoft.EntityFrameworkCore;
using RequestsManagement.Application.Requests;
using RequestsManagement.Domain.Requests;
using RequestsManagement.Infrastructure.Persistence;

namespace RequestsManagement.Infrastructure.Requests;

internal sealed class RequestRepository(RequestsDbContext dbContext) : IRequestRepository
{
    public Task<Request?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Requests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<int, Request>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken) =>
        await dbContext.Requests
            .Where(request => ids.Contains(request.Id))
            .ToDictionaryAsync(request => request.Id, cancellationToken);

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
            DiscardPendingChanges();
            return false;
        }
    }

    private void DiscardPendingChanges()
    {
        var pendingEntries = dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in pendingEntries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
