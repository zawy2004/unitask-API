using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMilestoneEscrowedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EscrowedAt",
                table: "Milestones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscrowedAt",
                table: "Milestones");
        }
    }
}
