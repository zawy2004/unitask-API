using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("JobId", Name = "idx_application_job")]
[Index("Status", Name = "idx_application_status")]
[Index("StudentId", Name = "idx_application_student")]
public partial class JobApplication
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid StudentId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public DateTime? AppliedAt { get; set; }

    public string? CoverLetter { get; set; }

    [StringLength(255)]
    public string? ProposedTimeline { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobApplications")]
    public virtual Job Job { get; set; } = null!;

    [InverseProperty("JobApplication")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("JobApplication")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("StudentId")]
    [InverseProperty("JobApplications")]
    public virtual StudentProfile Student { get; set; } = null!;
}
