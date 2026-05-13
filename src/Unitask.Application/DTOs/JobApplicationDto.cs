using System;

namespace Unitask.Application.DTOs;

public class JobApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid StudentId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? AppliedAt { get; set; }
    public string? CoverLetter { get; set; }
    public string? ProposedTimeline { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? StartedAt { get; set; }
}

