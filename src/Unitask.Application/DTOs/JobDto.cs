using System;

namespace Unitask.Application.DTOs;

public class JobDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? TagsJson { get; set; }
    public string Status { get; set; } = null!;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Currency { get; set; }
    public string? DurationType { get; set; }
    public int? DurationDays { get; set; }
    public string? RequiredSkillsJson { get; set; }
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

