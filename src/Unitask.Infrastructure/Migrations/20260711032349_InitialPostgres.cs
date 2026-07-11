using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unitask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    HelpfulCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    OrderIndex = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FAQs__3214EC07403668E9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    JobCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobCateg__3214EC07850F645F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Reasons = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Skills__3214EC073C6B2BBB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    UserType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "student"),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    ReputationScore = table.Column<int>(type: "integer", nullable: true, defaultValue: 100),
                    SuspendedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC076B717E97", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Activity__3214EC072A4CA5A1", x => x.Id);
                    table.ForeignKey(
                        name: "FK__ActivityL__UserI__367C1819",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Excerpt = table.Column<string>(type: "text", nullable: true),
                    FeaturedImageUrl = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TagsJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "draft"),
                    ViewCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    LikeCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    PublishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BlogPost__3214EC072AC3FACF", x => x.Id);
                    table.ForeignKey(
                        name: "FK__BlogPosts__Autho__24285DB4",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CompanyEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompanyWebsite = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompanySize = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "startup"),
                    Industry = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BusinessLicenseUrl = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompletedProjects = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    Balance = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    TotalSpent = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    Rating = table.Column<decimal>(type: "numeric(3,2)", nullable: true, defaultValue: 0m),
                    RejectionStrikes = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    IsPostingLocked = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Business__3214EC07C9C76523", x => x.Id);
                    table.ForeignKey(
                        name: "FK__BusinessP__UserI__5441852A",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Conversa__3214EC07003DAE44", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Conversat__User1__29E1370A",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Conversat__User2__2AD55B43",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    University = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Major = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GraduationYear = table.Column<int>(type: "integer", nullable: true),
                    CvUrl = table.Column<string>(type: "text", nullable: true),
                    GradePoint = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    StudentCardUrl = table.Column<string>(type: "text", nullable: true),
                    CitizenId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    PortfolioUrl = table.Column<string>(type: "text", nullable: true),
                    CompletedJobs = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TotalEarnings = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentP__3214EC07261DB460", x => x.Id);
                    table.ForeignKey(
                        name: "FK__StudentPr__UserI__47DBAE45",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TagsJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "open"),
                    SalaryMin = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    SalaryMax = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "VND"),
                    DurationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "micro"),
                    DurationDays = table.Column<int>(type: "integer", nullable: true),
                    RequiredSkillsJson = table.Column<string>(type: "text", nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "beginner"),
                    SpotsTotal = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    SpotsFilled = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsRemote = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    Deadline = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    PublishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Jobs__3214EC076D201E18", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Jobs__BusinessId__6A30C649",
                        column: x => x.BusinessId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Jobs__CategoryId__6B24EA82",
                        column: x => x.CategoryId,
                        principalTable: "JobCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    ReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Messages__3214EC07ECFE687E", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Messages__Conver__308E3499",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Messages__Sender__318258D2",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IssuingOrganization = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CredentialUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CredentialId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certifications_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Educations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Institution = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Degree = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    Gpa = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Educations_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProjectUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GithubUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioProjects_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Proficiency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "beginner"),
                    EndorsementCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    AddedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentS__3214EC076E534E1D", x => x.Id);
                    table.ForeignKey(
                        name: "FK__StudentSk__Skill__19DFD96B",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__StudentSk__Stude__18EBB532",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    TotalEarned = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    TotalWithdrawn = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentW__3214EC07C3EEA6EB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__StudentWa__Stude__7B5B524B",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    BankAccountName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Withdraw__3214EC07A06B1D8F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Withdrawa__Stude__0B91BA14",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ReportedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    ActionTaken = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AdminRep__3214EC0735BD4C61", x => x.Id);
                    table.ForeignKey(
                        name: "FK__AdminRepo__Repor__1975C517",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__AdminRepo__Repor__1A69E950",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__AdminRepo__Repor__1B5E0D89",
                        column: x => x.ReportedJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
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

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    AppliedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    ProposedTimeline = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobAppli__3214EC078CD78FF4", x => x.Id);
                    table.ForeignKey(
                        name: "FK__JobApplic__JobId__0697FACD",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__JobApplic__Stude__078C1F06",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RelatedJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    ReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3214EC077F7DF681", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Notificat__Relat__11D4A34F",
                        column: x => x.RelatedJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Notificat__Relat__12C8C788",
                        column: x => x.RelatedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__10E07F16",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EscrowedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
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

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    JobId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "VND"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__3214EC07BD9B1F00", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Payments__Busine__7FB5F314",
                        column: x => x.BusinessId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Payments__JobApp__7EC1CEDB",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Payments__JobId__7DCDAAA2",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Payments__Studen__00AA174D",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    SkillEndorsementsJson = table.Column<string>(type: "text", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reviews__3214EC07C82B75F3", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Reviews__FromUse__0A338187",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Reviews__JobAppl__093F5D4E",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Reviews__JobId__084B3915",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Reviews__ToUserI__0B27A5C0",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NEGOTIATION"),
                    MediatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StudentPercent = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    DecisionNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')"),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AppealDeadline = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    ClientFeedback = table.Column<string>(type: "text", nullable: true),
                    ClientEvidenceUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
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

            migrationBuilder.CreateIndex(
                name: "idx_activity_created_at",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_activity_user",
                table: "ActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_report_created_at",
                table: "AdminReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_report_status",
                table: "AdminReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdminReports_ReportedById",
                table: "AdminReports",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_AdminReports_ReportedJobId",
                table: "AdminReports",
                column: "ReportedJobId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminReports_ReportedUserId",
                table: "AdminReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "idx_blog_published_at",
                table: "BlogPosts",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "idx_blog_slug",
                table: "BlogPosts",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "idx_blog_status",
                table: "BlogPosts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorId",
                table: "BlogPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "UQ__BlogPost__BC7B5FB6BEECFCA5",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_business_industry",
                table: "BusinessProfiles",
                column: "Industry");

            migrationBuilder.CreateIndex(
                name: "idx_business_verified",
                table: "BusinessProfiles",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "UQ__Business__1788CC4DAC86ABAD",
                table: "BusinessProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_certification_student",
                table: "Certifications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "idx_contract_business",
                table: "Contracts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "idx_contract_job",
                table: "Contracts",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "idx_contract_status",
                table: "Contracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_contract_student",
                table: "Contracts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "idx_conversation_users",
                table: "Conversations",
                columns: new[] { "User1Id", "User2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id",
                table: "Conversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_milestone",
                table: "Disputes",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "idx_dispute_status",
                table: "Disputes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_education_student",
                table: "Educations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_UserId_Code",
                table: "EmailVerifications",
                columns: new[] { "UserId", "Code" });

            migrationBuilder.CreateIndex(
                name: "idx_faq_category",
                table: "FAQs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "idx_faq_order",
                table: "FAQs",
                column: "OrderIndex");

            migrationBuilder.CreateIndex(
                name: "idx_application_job",
                table: "JobApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "idx_application_status",
                table: "JobApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_application_student",
                table: "JobApplications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_Status",
                table: "JobApplications",
                columns: new[] { "JobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_StudentId_AppliedAt",
                table: "JobApplications",
                columns: new[] { "StudentId", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_category_slug",
                table: "JobCategories",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "UQ__JobCateg__BC7B5FB617A64871",
                table: "JobCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_job_business",
                table: "Jobs",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "idx_job_created_at",
                table: "Jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_job_deadline",
                table: "Jobs",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "idx_job_featured",
                table: "Jobs",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "idx_job_status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CategoryId",
                table: "Jobs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status_BusinessId",
                table: "Jobs",
                columns: new[] { "Status", "BusinessId" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status_CreatedAt",
                table: "Jobs",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_message_conversation",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "idx_message_is_read",
                table: "Messages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "idx_message_sender",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_CreatedAt",
                table: "Messages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Unread",
                table: "Messages",
                columns: new[] { "ConversationId", "IsRead", "SenderId" });

            migrationBuilder.CreateIndex(
                name: "idx_milestone_contract",
                table: "Milestones",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "idx_milestone_status",
                table: "Milestones",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_notification_created_at",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_notification_is_read",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "idx_notification_user",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedJobId",
                table: "Notifications",
                column: "RelatedJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedUserId",
                table: "Notifications",
                column: "RelatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_payment_application",
                table: "Payments",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "idx_payment_created_at",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_payment_status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BusinessId",
                table: "Payments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_JobId",
                table: "Payments",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "idx_portfolio_student",
                table: "PortfolioProjects",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "idx_portfolio_student_order",
                table: "PortfolioProjects",
                columns: new[] { "StudentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "idx_review_rating",
                table: "Reviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "idx_review_to_user",
                table: "Reviews",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FromUserId",
                table: "Reviews",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobApplicationId",
                table: "Reviews",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobId",
                table: "Reviews",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ToUserId_Rating",
                table: "Reviews",
                columns: new[] { "ToUserId", "Rating" });

            migrationBuilder.CreateIndex(
                name: "idx_skill_category",
                table: "Skills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "idx_skill_name",
                table: "Skills",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "UQ__Skills__737584F68FA901A4",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_student_major",
                table: "StudentProfiles",
                column: "Major");

            migrationBuilder.CreateIndex(
                name: "idx_student_university",
                table: "StudentProfiles",
                column: "University");

            migrationBuilder.CreateIndex(
                name: "idx_student_verified",
                table: "StudentProfiles",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "UQ__StudentP__1788CC4D8B81EDAD",
                table: "StudentProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_student_skill_skill",
                table: "StudentSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "idx_student_skill_student",
                table: "StudentSkills",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UQ__StudentS__5F3F22803C6777EE",
                table: "StudentSkills",
                columns: new[] { "StudentId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_wallet_student",
                table: "StudentWallets",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UQ__StudentW__32C52B980C342C3B",
                table: "StudentWallets",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_submission_milestone",
                table: "Submissions",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "idx_submission_student",
                table: "Submissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "idx_users_created_at",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "idx_users_user_type",
                table: "Users",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534FF2A9ABE",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_withdrawal_status",
                table: "WithdrawalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_withdrawal_student",
                table: "WithdrawalRequests",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "AdminReports");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "Educations");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "FAQs");

            migrationBuilder.DropTable(
                name: "MessageFlags");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PortfolioProjects");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "StudentSkills");

            migrationBuilder.DropTable(
                name: "StudentWallets");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");

            migrationBuilder.DropTable(
                name: "JobCategories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
