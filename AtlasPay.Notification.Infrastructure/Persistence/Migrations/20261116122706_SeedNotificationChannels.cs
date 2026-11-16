using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AtlasPay.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedNotificationChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "notification_channels",
                columns: new[] { "Channel", "IsActive", "UpdatedAt" },
                values: new object[,]
                {
                    { "Email", true, new DateTime(2026, 11, 16, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "Push", true, new DateTime(2026, 11, 16, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "Sms", true, new DateTime(2026, 11, 16, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "notification_channels",
                keyColumn: "Channel",
                keyValue: "Email");

            migrationBuilder.DeleteData(
                table: "notification_channels",
                keyColumn: "Channel",
                keyValue: "Push");

            migrationBuilder.DeleteData(
                table: "notification_channels",
                keyColumn: "Channel",
                keyValue: "Sms");
        }
    }
}
