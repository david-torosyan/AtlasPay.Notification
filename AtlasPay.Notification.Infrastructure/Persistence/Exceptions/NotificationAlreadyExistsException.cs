namespace AtlasPay.Notification.Application.Exceptions;

public sealed class NotificationAlreadyExistsException(Exception innerException)
    : Exception("Notification already exists.", innerException);