using AtlasPay.Notification.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace AtlasPay.Notification.Infrastructure.Persistence;

public sealed class UnitOfWorkTransaction(
    IDbContextTransaction transaction)
    : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken ct = default)
        => transaction.CommitAsync(ct);

    public ValueTask DisposeAsync()
        => transaction.DisposeAsync();
}