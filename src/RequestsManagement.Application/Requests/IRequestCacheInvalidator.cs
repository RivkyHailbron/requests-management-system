namespace RequestsManagement.Application.Requests;

public interface IRequestCacheInvalidator
{
    Task InvalidateAsync(CancellationToken cancellationToken);
}
