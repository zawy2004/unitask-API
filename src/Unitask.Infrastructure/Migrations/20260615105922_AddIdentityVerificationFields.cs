using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenId",
                table: "StudentProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCardUrl",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicenseUrl",
                table: "BusinessProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "BusinessProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "StudentCardUrl",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessLicenseUrl",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "BusinessProfiles");
        }
    }
}
