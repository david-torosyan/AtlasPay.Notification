using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_EventId_UserId_Type_AccountId",
                table: "notifications");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_EventId_UserId_Type_AccountId",
                table: "notifications",
                columns: new[] { "EventId", "UserId", "Type", "AccountId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_EventId_UserId_Type_AccountId",
                table: "notifications");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_EventId_UserId_Type_AccountId",
                table: "notifications",
                columns: new[] { "EventId", "UserId", "Type", "AccountId" },
                unique: true);
        }
    }
}
