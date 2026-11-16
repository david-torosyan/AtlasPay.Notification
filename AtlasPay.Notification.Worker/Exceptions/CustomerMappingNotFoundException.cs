namespace AtlasPay.Notification.Worker.Exceptions;

public sealed class CustomerMappingNotFoundException(Guid customerId)
    : Exception($"Customer mapping not found for {customerId}.")
{
    public Guid CustomerId { get; } = customerId;
}