using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;

namespace AtlasPay.Notification.Application.Features.DeferredEvents.Commands;

public sealed class DeferFinancialEventHandler(
    IDeferredEventRepository repository,
    IProcessedEventRepository processedEventRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        DeferFinancialEventCommand command,
        CancellationToken ct = default)
    {
        if (await processedEventRepository.ExistsAsync(command.EventId, ct))
            return;

        if (await repository.ExistsAsync(command.EventId, ct))
            return;

        repository.Add(new DeferredEvent(
            command.EventId,
            command.EventType,
            command.Payload));

        await unitOfWork.SaveChangesAsync(ct);
    }
}