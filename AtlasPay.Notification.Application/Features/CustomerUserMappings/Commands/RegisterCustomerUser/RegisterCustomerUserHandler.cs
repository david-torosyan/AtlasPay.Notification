using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
namespace AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;

public sealed class RegisterCustomerUserHandler(
    ICustomerUserMappingRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
       RegisterCustomerUserCommand command,
       CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email is required.", nameof(command.Email));

        var email = command.Email.Trim();

        var mapping = await repository.GetByCustomerIdAsync(command.CustomerId, ct);

        if (mapping is not null)
        {
            if (mapping.UserId != command.UserId)
                throw new InvalidOperationException(
                    $"Customer {command.CustomerId} is already mapped to another user.");

            if (mapping.Email == email)
                return;

            mapping.UpdateEmail(email);
            await unitOfWork.SaveChangesAsync(ct);
            return;
        }

        repository.Add(new CustomerUserMapping(
            command.CustomerId,
            command.UserId,
            email));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
