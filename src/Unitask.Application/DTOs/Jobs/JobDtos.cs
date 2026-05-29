using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Jobs;

public class JobCreateRequest
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? CategoryId { get; set; }

    public IReadOnlyList<string>? Tags { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Currency { get; set; }

    public string? DurationType { get; set; }

    public int? DurationDays { get; set; }

    public IReadOnlyList<string>? RequiredSkills { get; set; }

    public string? ExperienceLevel { get; set; }

    public int? SpotsTotal { get; set; }

    public string? Location { get; set; }

    public bool? IsRemote { get; set; }

    public DateTime? Deadline { get; set; }
}

public class JobUpdateRequest : JobCreateRequest
{
    public string? Status { get; set; }

    public bool? IsFeatured { get; set; }
}

public class JobListItemResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public Guid BusinessId { get; set; }

    public string? CompanyName { get; set; }

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public string Status { get; set; } = null!;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Currency { get; set; }

    public string? DurationType { get; set; }

    public int? DurationDays { get; set; }

    public IReadOnlyList<string> RequiredSkills { get; set; } = Array.Empty<string>();

    public string? ExperienceLevel { get; set; }

    public int? SpotsTotal { get; set; }

    public int? SpotsFilled { get; set; }

    public string? Location { get; set; }

    public bool? IsRemote { get; set; }

    public bool? IsFeatured { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}

public class JobCategoryInfoDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public int? JobCount { get; set; }
}

public class JobBusinessInfoDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public decimal? Rating { get; set; }
}

public class JobApplicationStudentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Rating { get; set; }
}

public class JobApplicationSummaryDto
{
    public Guid Id { get; set; }

    public string Status { get; set; } = null!;

    public JobApplicationStudentDto Student { get; set; } = new();

    public DateTime? AppliedAt { get; set; }
}

public class JobDetailsResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public JobCategoryInfoDto? Category { get; set; }

    public JobBusinessInfoDto? Business { get; set; }

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public string Status { get; set; } = null!;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public IReadOnlyList<string> RequiredSkills { get; set; } = Array.Empty<string>();

    public int? SpotsTotal { get; set; }

    public int? SpotsFilled { get; set; }

    public string? Location { get; set; }

    public bool? IsRemote { get; set; }

    public string? DurationType { get; set; }

    public int? DurationDays { get; set; }

    public DateTime? Deadline { get; set; }

    public IReadOnlyList<JobApplicationSummaryDto> Applications { get; set; } = Array.Empty<JobApplicationSummaryDto>();

    public DateTime? CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}

public class JobSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? CompanyName { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
