using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Unitask.Infrastructure.Persistence;

/// <summary>
/// Factory dùng ở design-time cho `dotnet ef` (migrations). Không cần chạy cả app host
/// (tránh phụ thuộc JwtSettings...). Chỉ cần provider Npgsql để sinh migration đúng dialect.
/// Connection string ở đây chỉ mang tính hình thức khi tạo migration (không kết nối thật).
/// </summary>
public class UnitaskDbContextFactory : IDesignTimeDbContextFactory<UnitaskDbContext>
{
    public UnitaskDbContext CreateDbContext(string[] args)
    {
        var conn = System.Environment.GetEnvironmentVariable("UNITASK_DB")
            ?? "Host=localhost;Database=unitask;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<UnitaskDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new UnitaskDbContext(options);
    }
}
