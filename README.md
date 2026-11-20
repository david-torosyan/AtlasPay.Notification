dotnet ef migrations add AddNotificationChanges --project AtlasPay.Notification.Infrastructure --startup-project AtlasPay.Notification.Api --context NotificationDbContext --output-dir Persistence/Migrations

dotnet ef database update --project AtlasPay.Notification.Infrastructure --startup-project AtlasPay.Notification.Api --context NotificationDbContext