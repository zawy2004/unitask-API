using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <summary>
    /// Migration cho module Hợp đồng & Milestone.
    ///
    /// LƯU Ý QUAN TRỌNG (baseline): DbContext này được scaffold từ một database đã tồn tại
    /// (DB-first) nên migration đầu tiên do EF sinh ra chứa TOÀN BỘ ~20 bảng cũ. Vì các bảng
    /// đó ĐÃ có sẵn trong DB, ta chỉ giữ lại trong Up()/Down() 3 bảng MỚI (Contracts,
    /// Milestones, Submissions). File ...ModelSnapshot.cs vẫn phản ánh đầy đủ toàn model để
    /// các migration sau này diff chính xác (các bảng cũ coi như đã được baseline).
    /// </summary>
    public partial class AddContractsMilestonesSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Contracts — FK tới các bảng đã tồn tại (Jobs, StudentProfiles, BusinessProfiles).
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_BusinessProfiles_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contracts_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contracts_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                });

            // 2) Milestones — FK tới Contracts (CASCADE: xóa hợp đồng kéo theo milestone).
            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Milestones_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 3) Submissions — FK tới Milestones (CASCADE) + StudentProfiles (NO ACTION).
            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                });

            // ----- Indexes -----
            migrationBuilder.CreateIndex(name: "idx_contract_business", table: "Contracts", column: "BusinessId");
            migrationBuilder.CreateIndex(name: "idx_contract_job", table: "Contracts", column: "JobId");
            migrationBuilder.CreateIndex(name: "idx_contract_status", table: "Contracts", column: "Status");
            migrationBuilder.CreateIndex(name: "idx_contract_student", table: "Contracts", column: "StudentId");

            migrationBuilder.CreateIndex(name: "idx_milestone_contract", table: "Milestones", column: "ContractId");
            migrationBuilder.CreateIndex(name: "idx_milestone_status", table: "Milestones", column: "Status");

            migrationBuilder.CreateIndex(name: "idx_submission_milestone", table: "Submissions", column: "MilestoneId");
            migrationBuilder.CreateIndex(name: "idx_submission_student", table: "Submissions", column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Thứ tự ngược: phụ thuộc trước (Submissions → Milestones → Contracts).
            migrationBuilder.DropTable(name: "Submissions");
            migrationBuilder.DropTable(name: "Milestones");
            migrationBuilder.DropTable(name: "Contracts");
        }
    }
}
