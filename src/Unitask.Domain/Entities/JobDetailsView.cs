using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Keyless]
public partial class JobDetailsView
{
    public Guid Id { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SalaryMin { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SalaryMax { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(50)]
    public string? DurationType { get; set; }

    public int? DurationDays { get; set; }

    [StringLength(50)]
    public string? ExperienceLevel { get; set; }

    public int? SpotsTotal { get; set; }

    public int? SpotsFilled { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }

    public bool? IsRemote { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(255)]
    public string CompanyName { get; set; } = null!;

    [StringLength(255)]
    public string CompanyContactName { get; set; } = null!;

    [StringLength(255)]
    public string CompanyEmail { get; set; } = null!;

    [StringLength(100)]
    public string? CategoryName { get; set; }

    public int? TotalApplications { get; set; }
}
