using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("BusinessId", Name = "idx_job_business")]
[Index("CreatedAt", Name = "idx_job_created_at")]
[Index("Deadline", Name = "idx_job_deadline")]
[Index("IsFeatured", Name = "idx_job_featured")]
[Index("Status", Name = "idx_job_status")]
public partial class Job
{
    [Key]
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid? CategoryId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? TagsJson { get; set; }

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

    public string? RequiredSkillsJson { get; set; }

    [StringLength(50)]
    public string? ExperienceLevel { get; set; }

    public int? SpotsTotal { get; set; }

    public int? SpotsFilled { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }

    public bool? IsRemote { get; set; }

    public bool? IsFeatured { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    [InverseProperty("ReportedJob")]
    public virtual ICollection<AdminReport> AdminReports { get; set; } = new List<AdminReport>();

    [ForeignKey("BusinessId")]
    [InverseProperty("Jobs")]
    public virtual BusinessProfile Business { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("Jobs")]
    public virtual JobCategory? Category { get; set; }

    [InverseProperty("Job")]
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    [InverseProperty("RelatedJob")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Job")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Job")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
