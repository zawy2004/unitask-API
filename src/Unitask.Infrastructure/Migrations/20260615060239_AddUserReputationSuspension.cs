using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserReputationSuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReputationScore",
                table: "Users",
                type: "int",
                nullable: true,
                defaultValue: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedUntil",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReputationScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SuspendedUntil",
                table: "Users");
        }
    }
}
