using System;

namespace Unitask.Application.DTOs.Reports;

/// <summary>Body cho POST /api/reports (BC1 — báo cáo vi phạm theo danh mục).</summary>
public class CreateReportRequest
{
    /// <summary>Danh mục: scam | nda | abuse | bypass | other (Lừa đảo / NDA / Xúc phạm / Bypass / Khác).</summary>
    public string ReportType { get; set; } = "other";

    public string Reason { get; set; } = string.Empty;

    /// <summary>Đối tượng bị báo cáo (tùy ngữ cảnh): user và/hoặc job.</summary>
    public Guid? ReportedUserId { get; set; }
    public Guid? ReportedJobId { get; set; }
}

public class ReportResponse
{
    public Guid Id { get; set; }
    public string? ReportType { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? CreatedAt { get; set; }
}
