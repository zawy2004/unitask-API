using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Portfolio;

// === Portfolio Project ===

public class PortfolioProjectResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? Tags { get; set; }
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsHighlighted { get; set; }
    public int SortOrder { get; set; }
}

public class PortfolioProjectRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ProjectUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string? Tags { get; set; }
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsHighlighted { get; set; }
    public int SortOrder { get; set; }
}

// === Education ===

public class EducationResponse
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = null!;
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public decimal? Gpa { get; set; }
    public string? Description { get; set; }
    public bool? IsCurrent { get; set; }
    public int SortOrder { get; set; }
}

public class EducationRequest
{
    public string Institution { get; set; } = null!;
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public decimal? Gpa { get; set; }
    public string? Description { get; set; }
    public bool? IsCurrent { get; set; }
    public int SortOrder { get; set; }
}

// === Certification ===

public class CertificationResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? CredentialUrl { get; set; }
    public string? CredentialId { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}

public class CertificationRequest
{
    public string Name { get; set; } = null!;
    public string? IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? CredentialUrl { get; set; }
    public string? CredentialId { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}

// === Full Portfolio (Public View) ===

public class PortfolioPublicResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Title { get; set; }
    public string? University { get; set; }
    public string? Major { get; set; }
    public int? GraduationYear { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? CvUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool? IsVerified { get; set; }
    public int CompletedJobs { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public IReadOnlyList<PortfolioSkillDto> Skills { get; set; } = Array.Empty<PortfolioSkillDto>();
    public IReadOnlyList<PortfolioProjectResponse> Projects { get; set; } = Array.Empty<PortfolioProjectResponse>();
    public IReadOnlyList<EducationResponse> Educations { get; set; } = Array.Empty<EducationResponse>();
    public IReadOnlyList<CertificationResponse> Certifications { get; set; } = Array.Empty<CertificationResponse>();
    public IReadOnlyList<PortfolioReviewDto> Reviews { get; set; } = Array.Empty<PortfolioReviewDto>();
}

public class PortfolioSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public string? Proficiency { get; set; }
    public int EndorsementCount { get; set; }
}

public class PortfolioReviewDto
{
    public Guid Id { get; set; }
    public string? ReviewerName { get; set; }
    public string? ReviewerAvatar { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? CreatedAt { get; set; }
}
