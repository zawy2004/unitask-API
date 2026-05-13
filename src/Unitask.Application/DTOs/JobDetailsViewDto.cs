using System;

namespace Unitask.Application.DTOs;

public class JobDetailsViewDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Currency { get; set; }
    public string? DurationType { get; set; }
    public int? DurationDays { get; set; }
    public string? ExperienceLevel { get; set; }
    public int? SpotsTotal { get; set; }
    public int? SpotsFilled { get; set; }
    public string? Location { get; set; }
    public bool? IsRemote { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CompanyName { get; set; } = null!;
    public string CompanyContactName { get; set; } = null!;
    public string CompanyEmail { get; set; } = null!;
    public string? CategoryName { get; set; }
    public int? TotalApplications { get; set; }
}

