using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Businesses;

public class BusinessProfileResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? CompanyEmail { get; set; }

    public string? CompanyWebsite { get; set; }

    public string? CompanySize { get; set; }

    public string? Industry { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int? CompletedProjects { get; set; }

    public decimal? Balance { get; set; }

    public decimal? TotalSpent { get; set; }

    public decimal? Rating { get; set; }

    public int? RejectionStrikes { get; set; }

    public bool? IsPostingLocked { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? Address { get; set; }

    public string? TaxCode { get; set; }

    public string? BusinessLicenseUrl { get; set; }
}

/// <summary>Body xác thực doanh nghiệp: Mã số thuế + giấy phép kinh doanh.</summary>
public class BusinessVerifyRequest
{
    public string? TaxCode { get; set; }
    public string? BusinessLicenseUrl { get; set; }
}

public class BusinessUpdateRequest
{
    public string? CompanyName { get; set; }

    public string? CompanyEmail { get; set; }

    public string? CompanyWebsite { get; set; }

    public string? CompanySize { get; set; }

    public string? Industry { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? Address { get; set; }
}

public class BusinessStatsResponse
{
    public int OpenJobs { get; set; }

    public int TotalApplications { get; set; }

    public int PendingApplications { get; set; }

    public decimal TotalSpent { get; set; }

    public int CompletedProjects { get; set; }

    public decimal AverageRating { get; set; }
}

public class BusinessDashboardApplicationDto
{
    public Guid Id { get; set; }

    public string Status { get; set; } = null!;

    public Guid JobId { get; set; }

    public string JobTitle { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public DateTime? AppliedAt { get; set; }
}

public class BusinessDashboardJobDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? SpotsFilled { get; set; }

    public int? SpotsTotal { get; set; }
}

public class BusinessDashboardResponse
{
    public BusinessProfileResponse Business { get; set; } = new();

    public BusinessStatsResponse Stats { get; set; } = new();

    public IReadOnlyList<BusinessDashboardApplicationDto> RecentApplications { get; set; } = Array.Empty<BusinessDashboardApplicationDto>();

    public IReadOnlyList<BusinessDashboardJobDto> OpenJobs { get; set; } = Array.Empty<BusinessDashboardJobDto>();

    public IReadOnlyList<StudentDashboardNotificationDto> Notifications { get; set; } = Array.Empty<StudentDashboardNotificationDto>();
}
