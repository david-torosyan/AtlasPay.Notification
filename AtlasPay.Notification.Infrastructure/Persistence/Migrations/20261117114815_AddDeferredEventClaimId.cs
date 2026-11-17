using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeferredEventClaimId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "deferred_events",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "deferred_events");
        }
    }
}
