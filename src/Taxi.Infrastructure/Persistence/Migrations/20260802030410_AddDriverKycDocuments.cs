using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverKycDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_document_key",
                table: "drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_document_key",
                table: "drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vehicle_registration_document_key",
                table: "drivers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_document_key",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "license_document_key",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "drivers");

            migrationBuilder.DropColumn(
                name: "vehicle_registration_document_key",
                table: "drivers");
        }
    }
}
