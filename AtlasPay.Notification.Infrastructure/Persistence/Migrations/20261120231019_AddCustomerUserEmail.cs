using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "customer_users",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "customer_users");
        }
    }
}
