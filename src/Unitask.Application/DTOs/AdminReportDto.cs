using System;

namespace Unitask.Application.DTOs;

public class AdminReportDto
{
    public Guid Id { get; set; }
    public Guid ReportedById { get; set; }
    public Guid? ReportedUserId { get; set; }
    public Guid? ReportedJobId { get; set; }
    public string? ReportType { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ActionTaken { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

