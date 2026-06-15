using Unitask.Application.Common.Interfaces;

namespace Unitask.Api.Jobs;

/// <summary>
/// CHÍNH SÁCH ESCROW 1.2 — "im lặng = chấp thuận".
///
/// Hosted background service chạy định kỳ, gọi <see cref="IMilestoneService.AutoReleaseExpiredAsync"/>
/// để tự động nghiệm thu &amp; giải ngân các milestone đang chờ duyệt (UNDER_REVIEW) mà doanh nghiệp
/// không phản hồi quá <see cref="TimeoutHours"/> giờ kể từ lần nộp gần nhất — bảo vệ người thực hiện
/// khỏi bị treo tiền vô thời hạn.
///
/// IMilestoneService là scoped (gắn DbContext) nên mỗi vòng phải tạo scope riêng.
/// </summary>
public class EscrowAutoReleaseService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EscrowAutoReleaseService> _logger;

    /// <summary>Ngưỡng "im lặng = chấp thuận": 72 giờ.</summary>
    private const int TimeoutHours = 72;

    /// <summary>Tần suất quét (kiểm tra mỗi giờ là đủ cho ngưỡng 72h).</summary>
    private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(1);

    public EscrowAutoReleaseService(IServiceProvider services, ILogger<EscrowAutoReleaseService> logger)
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
                using var scope = _services.CreateScope();
                var milestoneService = scope.ServiceProvider.GetRequiredService<IMilestoneService>();
                var released = await milestoneService.AutoReleaseExpiredAsync(TimeoutHours, stoppingToken);
                if (released > 0)
                    _logger.LogInformation("Escrow 1.2: tự động giải ngân {Count} milestone (quá {Hours}h).", released, TimeoutHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Escrow auto-release job failed");
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
}
