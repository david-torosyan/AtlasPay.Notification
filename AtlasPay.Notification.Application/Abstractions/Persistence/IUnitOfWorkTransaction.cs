namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}