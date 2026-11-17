using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AtlasPay.Notification.Infrastructure.Persistence.Exceptions;

public static class NotificationDuplicateDetector
{
    private const string UniqueIndexName =
        "IX_notifications_EventId_UserId_Type_AccountId";

    public static bool IsDuplicate(DbUpdateException ex)
        => ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: UniqueIndexName
        };
}