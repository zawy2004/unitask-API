using Microsoft.EntityFrameworkCore;
using Unitask.Infrastructure.Persistence;

// DateTime từ SQL Server có Kind=Unspecified — cho phép ghi vào Postgres.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var azure = Environment.GetEnvironmentVariable("AZURE_DB");
var supa = Environment.GetEnvironmentVariable("SUPABASE_DB");
if (string.IsNullOrWhiteSpace(azure) || string.IsNullOrWhiteSpace(supa))
{
    Console.Error.WriteLine("Cần env AZURE_DB (SqlServer) và SUPABASE_DB (Npgsql).");
    return 1;
}

var srcOptions = new DbContextOptionsBuilder<UnitaskDbContext>().UseSqlServer(azure).Options;
var dstOptions = new DbContextOptionsBuilder<UnitaskDbContext>().UseNpgsql(supa).Options;

await using var src = new UnitaskDbContext(srcOptions);
await using var dst = new UnitaskDbContext(dstOptions);

Console.WriteLine("== Bước 1: đảm bảo schema trên Supabase (migrate + views/triggers) ==");
await dst.Database.MigrateAsync();
await dst.Database.ExecuteSqlRawAsync(PostgresDbObjects.ViewsAndTriggers);
Console.WriteLine("   schema OK");

Console.WriteLine("== Bước 2: tắt kiểm tra FK/trigger khi nạp ==");
var replicaOk = false;
try
{
    await dst.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");
    replicaOk = true;
    Console.WriteLine("   session_replication_role = replica");
}
catch (Exception ex)
{
    Console.WriteLine($"   không set được replica ({ex.Message.Split('\n')[0]}); dựa vào thứ tự chèn cha-trước.");
}

Console.WriteLine("== Bước 3: copy dữ liệu Azure -> Supabase ==");
var total = 0;

async Task Copy<T>(Func<UnitaskDbContext, DbSet<T>> set, string name) where T : class
{
    List<T> rows;
    try { rows = await set(src).AsNoTracking().ToListAsync(); }
    catch (Exception ex) { Console.WriteLine($"   {name,-22} ĐỌC LỖI: {(ex.InnerException?.Message ?? ex.Message).Split('\n')[0]}"); return; }

    int ok = 0; string firstErr = null;
    foreach (var row in rows)
    {
        try { set(dst).Add(row); await dst.SaveChangesAsync(); ok++; }
        catch (Exception ex) { firstErr ??= (ex.InnerException?.Message ?? ex.Message).Split('\n')[0]; }
        finally { dst.ChangeTracker.Clear(); }
    }
    total += ok;
    Console.WriteLine($"   {name,-22} {ok}/{rows.Count}" + (firstErr != null ? $"  (lỗi: {firstErr})" : ""));
}

// Thứ tự cha-trước (đảm bảo FK kể cả khi không tắt được replica).
await Copy(c => c.Users, "Users");
await Copy(c => c.JobCategories, "JobCategories");
await Copy(c => c.Skills, "Skills");
await Copy(c => c.StudentProfiles, "StudentProfiles");
await Copy(c => c.BusinessProfiles, "BusinessProfiles");
await Copy(c => c.StudentWallets, "StudentWallets");
await Copy(c => c.Jobs, "Jobs");
await Copy(c => c.StudentSkills, "StudentSkills");
await Copy(c => c.JobApplications, "JobApplications");
await Copy(c => c.Payments, "Payments");
await Copy(c => c.Reviews, "Reviews");
await Copy(c => c.Conversations, "Conversations");
await Copy(c => c.Messages, "Messages");
await Copy(c => c.MessageFlags, "MessageFlags");
await Copy(c => c.Notifications, "Notifications");
await Copy(c => c.ActivityLogs, "ActivityLogs");
await Copy(c => c.AdminReports, "AdminReports");
await Copy(c => c.BlogPosts, "BlogPosts");
await Copy(c => c.FAQs, "FAQs");
await Copy(c => c.WithdrawalRequests, "WithdrawalRequests");
await Copy(c => c.PortfolioProjects, "PortfolioProjects");
await Copy(c => c.Educations, "Educations");
await Copy(c => c.Certifications, "Certifications");
await Copy(c => c.EmailVerifications, "EmailVerifications");
await Copy(c => c.Contracts, "Contracts");
await Copy(c => c.Milestones, "Milestones");
await Copy(c => c.Submissions, "Submissions");
await Copy(c => c.Disputes, "Disputes");

if (replicaOk)
{
    try { await dst.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;"); } catch { }
}

Console.WriteLine($"== XONG: tổng {total} dòng đã copy ==");
return 0;
