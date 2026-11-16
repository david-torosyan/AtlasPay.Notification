using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;

namespace AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Commands;

public sealed class SetAccountNotificationPreferenceHandler(
    IAccountNotificationPreferenceRepository repository,
    INotificationChannelRepository channelRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        SetAccountNotificationPreferenceCommand command,
        CancellationToken ct = default)
    {
        var channel = await channelRepository.GetAsync(
            command.Channel, ct);

        if (channel is null)
            throw new InvalidOperationException(
                "Notification channel does not exist.");

        var preference = await repository.GetAsync(
                command.UserId,
                command.AccountId,
                command.NotificationType,
                command.Channel,
                ct);

        if (preference is null)
        {
            repository.Add(new AccountNotificationPreference(
                command.UserId,
                command.AccountId,
                command.NotificationType,
                command.Channel,
                command.Enabled));
        }
        else
        {
            preference.SetEnabled(command.Enabled);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}