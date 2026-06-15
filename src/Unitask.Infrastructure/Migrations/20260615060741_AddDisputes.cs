using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaisedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NEGOTIATION"),
                    MediatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StudentPercent = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DecisionNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealDeadline = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputes_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_dispute_milestone",
                table: "Disputes",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_status",
                table: "Disputes",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Disputes");
        }
    }
}
