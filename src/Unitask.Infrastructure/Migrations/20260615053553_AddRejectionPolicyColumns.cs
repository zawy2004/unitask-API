using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionPolicyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientEvidenceUrl",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPostingLocked",
                table: "BusinessProfiles",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RejectionStrikes",
                table: "BusinessProfiles",
                type: "int",
                nullable: true,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientEvidenceUrl",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IsPostingLocked",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionStrikes",
                table: "BusinessProfiles");
        }
    }
}
