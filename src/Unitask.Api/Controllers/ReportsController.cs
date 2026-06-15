using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Reports;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

/// <summary>
/// BC1 — Báo cáo vi phạm. Người dùng đã đăng nhập gửi báo cáo (tin nhắn/task/profile)
/// theo danh mục; lưu vào dbo.AdminReports với Status = 'pending' để admin xử lý.
/// ReportedById lấy từ JWT (không tin client).
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly UnitaskDbContext _db;

    public ReportsController(UnitaskDbContext db)
    {
        _db = db;
    }

    private static readonly string[] AllowedTypes = { "scam", "nda", "abuse", "bypass", "other" };

    [HttpPost]
    public async Task<ActionResult<ReportResponse>> Create([FromBody] CreateReportRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request?.Reason))
            return BadRequest(new { message = "Vui lòng nhập lý do báo cáo." });

        var type = (request.ReportType ?? "other").ToLowerInvariant();
        if (Array.IndexOf(AllowedTypes, type) < 0) type = "other";

        var report = new AdminReport
        {
            Id = Guid.NewGuid(),
            ReportedById = userId.Value,
            ReportedUserId = request.ReportedUserId,
            ReportedJobId = request.ReportedJobId,
            ReportType = type,
            Reason = request.Reason.Trim(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.AdminReports.Add(report);
        await _db.SaveChangesAsync(ct);

        return Ok(new ReportResponse
        {
            Id = report.Id,
            ReportType = report.ReportType,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        });
    }
}
