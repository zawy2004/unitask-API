using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Applications;

public class ApplyJobRequest
{
    public string? CoverLetter { get; set; }

    public string? ProposedTimeline { get; set; }
}

public class ApplicationStudentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? University { get; set; }

    public decimal? Rating { get; set; }

    public int? CompletedJobs { get; set; }
}

public class JobApplicationListItemResponse
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid StudentId { get; set; }

    public ApplicationStudentDto Student { get; set; } = new();

    public string Status { get; set; } = null!;

    public string? CoverLetter { get; set; }

    public string? ProposedTimeline { get; set; }

    public DateTime? AppliedAt { get; set; }
}

public class JobApplicationResponse
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid StudentId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? AppliedAt { get; set; }
}

public class JobApplicationAcceptRequest
{
    public DateTime? StartDate { get; set; }
}

public class JobApplicationRejectRequest
{
    public string RejectionReason { get; set; } = null!;
}

public class MyApplicationJobDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Company { get; set; }

    public string? Salary { get; set; }
}

public class MyApplicationResponse
{
    public Guid Id { get; set; }

    public MyApplicationJobDto Job { get; set; } = new();

    public string Status { get; set; } = null!;

    public DateTime? AppliedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }
}
