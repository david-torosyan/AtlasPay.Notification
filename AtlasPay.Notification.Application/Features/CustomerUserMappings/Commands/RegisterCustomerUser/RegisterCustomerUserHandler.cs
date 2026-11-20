using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;

public sealed class RegisterCustomerUserHandler(
    ICustomerUserMappingRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
       RegisterCustomerUserCommand command,
       CancellationToken ct)
    {
        var mapping = await repository.GetByCustomerIdAsync(command.CustomerId, ct);

        if (mapping is not null)
        {
            if (mapping.UserId != command.UserId)
                throw new InvalidOperationException(
                    $"Customer {command.CustomerId} is already mapped to another user.");

            return;
        }

        repository.Add(new CustomerUserMapping(
            command.CustomerId,
            command.UserId));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
