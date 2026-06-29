using Microsoft.EntityFrameworkCore;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Jobs;

/// <summary>
/// LOGIC THỜI GIAN (Giai đoạn 3) — tự động hết hạn Job.
///
/// Hosted background service quét database mỗi giờ: mọi Job đang mở (open/published)
/// có <c>Deadline &lt; hiện tại</c> sẽ được chuyển trạng thái sang <c>"expired"</c>.
/// Nhờ vậy listing &amp; jobCount phản ánh đúng thực tế thay vì chỉ tính tạm ở frontend,
/// đồng thời job hết hạn tự rời khỏi danh sách "đang tuyển".
///
/// DbContext là scoped nên mỗi vòng phải tạo scope riêng.
/// </summary>
public class JobExpiryService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<JobExpiryService> _logger;

    /// <summary>Tần suất quét: mỗi giờ.</summary>
    private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(1);

    public JobExpiryService(IServiceProvider services, ILogger<JobExpiryService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireOverdueJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobExpiry service failed");
            }

            try
            {
                await Task.Delay(ScanInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break; // service đang dừng
            }
        }
    }

    private async Task ExpireOverdueJobsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UnitaskDbContext>();

        var now = DateTime.UtcNow;
        var overdue = await db.Jobs
            .Where(j => (j.Status == "open" || j.Status == "published")
                        && j.Deadline != null
                        && j.Deadline < now)
            .ToListAsync(ct);

        if (overdue.Count == 0) return;

        foreach (var job in overdue)
        {
            job.Status = "expired";
            job.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("JobExpiry: đã chuyển {Count} job quá hạn sang trạng thái 'expired'.", overdue.Count);
    }
}
