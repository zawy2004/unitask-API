using System;

namespace Unitask.Application.DTOs;

public class BusinessDashboardViewDto
{
    public Guid BusinessId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? Industry { get; set; }
    public int? CompletedProjects { get; set; }
    public decimal? TotalSpent { get; set; }
    public decimal? Rating { get; set; }
    public int? OpenJobs { get; set; }
    public int? PendingApplications { get; set; }
}

