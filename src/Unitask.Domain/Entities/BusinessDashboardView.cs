using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Keyless]
public partial class BusinessDashboardView
{
    public Guid BusinessId { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(255)]
    public string CompanyName { get; set; } = null!;

    [StringLength(255)]
    public string? Industry { get; set; }

    public int? CompletedProjects { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalSpent { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? Rating { get; set; }

    public int? OpenJobs { get; set; }

    public int? PendingApplications { get; set; }
}
