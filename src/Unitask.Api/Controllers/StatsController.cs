using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

/// <summary>
/// Thống kê nền tảng (công khai, cho trang chủ) — đếm SỐ THẬT thay vì suy diễn tỉ lệ.
/// Chỉ tính tài khoản đang hoạt động: <c>IsActive == true</c> và không bị đình chỉ
/// → tài khoản bị admin vô hiệu hóa/khóa KHÔNG được tính.
/// </summary>
[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public StatsController(UnitaskDbContext dbContext) => _dbContext = dbContext;

    public class PlatformStatsDto
    {
        public int TotalJobs { get; set; }
        public int TotalBusinesses { get; set; }
        public int TotalStudents { get; set; }
    }

    [HttpGet("platform")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<PlatformStatsDto>> GetPlatform()
    {
        try
        {
            var now = DateTime.UtcNow;

            var totalJobs = await _dbContext.Jobs
                .CountAsync(j => j.Status == "open");

            var totalBusinesses = await _dbContext.Users.CountAsync(u =>
                u.UserType == "business"
                && u.IsActive == true
                && (u.SuspendedUntil == null || u.SuspendedUntil < now));

            var totalStudents = await _dbContext.Users.CountAsync(u =>
                u.UserType == "student"
                && u.IsActive == true
                && (u.SuspendedUntil == null || u.SuspendedUntil < now));

            return Ok(new PlatformStatsDto
            {
                TotalJobs = totalJobs,
                TotalBusinesses = totalBusinesses,
                TotalStudents = totalStudents
            });
        }
        catch
        {
            return Ok(new PlatformStatsDto { TotalJobs = 0, TotalBusinesses = 0, TotalStudents = 0 });
        }
    }
}
