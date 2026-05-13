using System;

namespace Unitask.Application.DTOs;

public class BusinessProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? CompanyEmail { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanySize { get; set; }
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? CompletedProjects { get; set; }
    public decimal? TotalSpent { get; set; }
    public decimal? Rating { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

