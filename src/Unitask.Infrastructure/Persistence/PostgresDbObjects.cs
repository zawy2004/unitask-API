namespace Unitask.Infrastructure.Persistence;

/// <summary>
/// Các đối tượng DB mà EF Migrations không quản lý: 3 view dashboard + 9 trigger
/// tự cập nhật UpdatedAt. Viết bằng cú pháp PostgreSQL, idempotent (chạy lại an toàn).
/// Định danh phải để nguyên PascalCase trong dấu ngoặc kép để khớp cách Npgsql sinh SQL.
/// </summary>
public static class PostgresDbObjects
{
    public const string ViewsAndTriggers = """
-- ===== Trigger tự cập nhật UpdatedAt (BEFORE UPDATE, idiomatic Postgres) =====
CREATE OR REPLACE FUNCTION set_updated_at_utc() RETURNS trigger AS $$
BEGIN
    NEW."UpdatedAt" := (now() at time zone 'utc');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_users_updatedat ON "Users";
CREATE TRIGGER trg_users_updatedat BEFORE UPDATE ON "Users"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_studentprofiles_updatedat ON "StudentProfiles";
CREATE TRIGGER trg_studentprofiles_updatedat BEFORE UPDATE ON "StudentProfiles"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_businessprofiles_updatedat ON "BusinessProfiles";
CREATE TRIGGER trg_businessprofiles_updatedat BEFORE UPDATE ON "BusinessProfiles"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_jobs_updatedat ON "Jobs";
CREATE TRIGGER trg_jobs_updatedat BEFORE UPDATE ON "Jobs"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_payments_updatedat ON "Payments";
CREATE TRIGGER trg_payments_updatedat BEFORE UPDATE ON "Payments"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_reviews_updatedat ON "Reviews";
CREATE TRIGGER trg_reviews_updatedat BEFORE UPDATE ON "Reviews"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_blogposts_updatedat ON "BlogPosts";
CREATE TRIGGER trg_blogposts_updatedat BEFORE UPDATE ON "BlogPosts"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_faqs_updatedat ON "FAQs";
CREATE TRIGGER trg_faqs_updatedat BEFORE UPDATE ON "FAQs"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_conversations_updatedat ON "Conversations";
CREATE TRIGGER trg_conversations_updatedat BEFORE UPDATE ON "Conversations"
    FOR EACH ROW EXECUTE FUNCTION set_updated_at_utc();

-- ===== Views dashboard =====
CREATE OR REPLACE VIEW "StudentDashboardView" AS
SELECT
    sp."Id" AS "StudentId",
    u."Email",
    u."FullName",
    sp."University",
    sp."Major",
    sp."CompletedJobs",
    sp."TotalEarnings",
    sw."Balance",
    (SELECT COUNT(*)::int FROM "JobApplications" WHERE "StudentId" = sp."Id" AND "Status" = 'pending') AS "PendingApplications",
    (SELECT COUNT(*)::int FROM "JobApplications" WHERE "StudentId" = sp."Id" AND "Status" = 'accepted') AS "ActiveJobs"
FROM "StudentProfiles" sp
INNER JOIN "Users" u ON sp."UserId" = u."Id"
LEFT JOIN "StudentWallets" sw ON sp."Id" = sw."StudentId";

CREATE OR REPLACE VIEW "BusinessDashboardView" AS
SELECT
    bp."Id" AS "BusinessId",
    u."Email",
    u."FullName",
    bp."CompanyName",
    bp."Industry",
    bp."CompletedProjects",
    bp."TotalSpent",
    bp."Rating",
    (SELECT COUNT(*)::int FROM "Jobs" WHERE "BusinessId" = bp."Id" AND "Status" = 'open') AS "OpenJobs",
    (SELECT COUNT(*)::int FROM "JobApplications" ja
        INNER JOIN "Jobs" j ON ja."JobId" = j."Id"
        WHERE j."BusinessId" = bp."Id" AND ja."Status" = 'pending') AS "PendingApplications"
FROM "BusinessProfiles" bp
INNER JOIN "Users" u ON bp."UserId" = u."Id";

CREATE OR REPLACE VIEW "JobDetailsView" AS
SELECT
    j."Id",
    j."Title",
    j."Description",
    j."Status",
    j."SalaryMin",
    j."SalaryMax",
    j."Currency",
    j."DurationType",
    j."DurationDays",
    j."ExperienceLevel",
    j."SpotsTotal",
    j."SpotsFilled",
    j."Location",
    j."IsRemote",
    j."Deadline",
    j."CreatedAt",
    j."UpdatedAt",
    bp."CompanyName",
    u."FullName" AS "CompanyContactName",
    u."Email" AS "CompanyEmail",
    jc."Name" AS "CategoryName",
    (SELECT COUNT(*)::int FROM "JobApplications" WHERE "JobId" = j."Id") AS "TotalApplications"
FROM "Jobs" j
INNER JOIN "BusinessProfiles" bp ON j."BusinessId" = bp."Id"
INNER JOIN "Users" u ON bp."UserId" = u."Id"
LEFT JOIN "JobCategories" jc ON j."CategoryId" = jc."Id";
""";
}
