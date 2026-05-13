using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Keyless]
public partial class StudentDashboardView
{
    public Guid StudentId { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(255)]
    public string? University { get; set; }

    [StringLength(255)]
    public string? Major { get; set; }

    public int? CompletedJobs { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalEarnings { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? Balance { get; set; }

    public int? PendingApplications { get; set; }

    public int? ActiveJobs { get; set; }
}
