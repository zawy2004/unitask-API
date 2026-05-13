using System;

namespace Unitask.Application.DTOs;

public class StudentProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? StudentEmail { get; set; }
    public string? University { get; set; }
    public string? Major { get; set; }
    public int? GraduationYear { get; set; }
    public string? CvUrl { get; set; }
    public decimal? GradePoint { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public int? CompletedJobs { get; set; }
    public decimal? TotalEarnings { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

