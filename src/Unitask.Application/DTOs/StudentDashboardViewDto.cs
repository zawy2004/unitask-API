using System;

namespace Unitask.Application.DTOs;

public class StudentDashboardViewDto
{
    public Guid StudentId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? University { get; set; }
    public string? Major { get; set; }
    public int? CompletedJobs { get; set; }
    public decimal? TotalEarnings { get; set; }
    public decimal? Balance { get; set; }
    public int? PendingApplications { get; set; }
    public int? ActiveJobs { get; set; }
}

