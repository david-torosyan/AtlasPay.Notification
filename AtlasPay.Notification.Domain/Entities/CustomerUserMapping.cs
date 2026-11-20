namespace AtlasPay.Notification.Domain.Entities;

public sealed class CustomerUserMapping
{
    private CustomerUserMapping() { } // For EF Core

    public Guid CustomerId { get; private set; }
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;

    public CustomerUserMapping(Guid customerId, Guid userId, string email)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        CustomerId = customerId;
        UserId = userId;
        SetEmail(email);
    }

    public void UpdateUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        UserId = userId;
    }

    public void UpdateEmail(string email)
        => SetEmail(email);

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        var normalizedEmail = email.Trim();

        if (normalizedEmail.Length > 320)
            throw new ArgumentException(
                "Email cannot exceed 320 characters.",
                nameof(email));

        Email = normalizedEmail;
    }
}
