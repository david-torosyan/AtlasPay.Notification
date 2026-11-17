using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeferredEventLeasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deferred_events_NextRetryAt",
                table: "deferred_events");

            migrationBuilder.AddColumn<string>(
                name: "LockedBy",
                table: "deferred_events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "deferred_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_deferred_events_NextRetryAt_LockedUntil",
                table: "deferred_events",
                columns: new[] { "NextRetryAt", "LockedUntil" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deferred_events_NextRetryAt_LockedUntil",
                table: "deferred_events");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "deferred_events");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "deferred_events");

            migrationBuilder.CreateIndex(
                name: "IX_deferred_events_NextRetryAt",
                table: "deferred_events",
                column: "NextRetryAt");
        }
    }
}
