using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unitask.Api.Extensions;
using Unitask.Infrastructure.Services;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IGoogleAnalyticsService _analytics;

    public AdminAnalyticsController(IGoogleAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] int days = 28)
    {
        if (!string.Equals(User.GetUserRole(), "admin", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        days = Math.Clamp(days, 7, 90);

        try
        {
            var data = await _analytics.GetOverviewAsync(days, HttpContext.RequestAborted);
            return Ok(data);
        }
        catch (AnalyticsNotConfiguredException)
        {
            return Ok(new Unitask.Application.DTOs.Analytics.AnalyticsOverviewDto { Configured = false });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Không lấy được dữ liệu GA4.", detail = ex.Message });
        }
    }
}
