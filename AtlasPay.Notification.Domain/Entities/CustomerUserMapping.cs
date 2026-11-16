namespace AtlasPay.Notification.Domain.Entities;

public sealed class CustomerUserMapping
{
    private CustomerUserMapping() { } // For EF Core

    public Guid CustomerId { get; private set; }
    public Guid UserId { get; private set; }

    public CustomerUserMapping(Guid customerId, Guid userId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        CustomerId = customerId;
        UserId = userId;
    }

    public void UpdateUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        UserId = userId;
    }
}