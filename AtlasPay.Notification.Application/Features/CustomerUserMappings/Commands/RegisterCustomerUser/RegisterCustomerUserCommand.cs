namespace AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;

public sealed record RegisterCustomerUserCommand(
    Guid CustomerId,
    Guid UserId,
    string Email);
