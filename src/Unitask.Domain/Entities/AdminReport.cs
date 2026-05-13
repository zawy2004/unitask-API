using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("CreatedAt", Name = "idx_report_created_at")]
[Index("Status", Name = "idx_report_status")]
public partial class AdminReport
{
    [Key]
    public Guid Id { get; set; }

    public Guid ReportedById { get; set; }

    public Guid? ReportedUserId { get; set; }

    public Guid? ReportedJobId { get; set; }

    [StringLength(100)]
    public string? ReportType { get; set; }

    public string Reason { get; set; } = null!;

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public string? ActionTaken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [ForeignKey("ReportedById")]
    [InverseProperty("AdminReportReportedBies")]
    public virtual User ReportedBy { get; set; } = null!;

    [ForeignKey("ReportedJobId")]
    [InverseProperty("AdminReports")]
    public virtual Job? ReportedJob { get; set; }

    [ForeignKey("ReportedUserId")]
    [InverseProperty("AdminReportReportedUsers")]
    public virtual User? ReportedUser { get; set; }
}
