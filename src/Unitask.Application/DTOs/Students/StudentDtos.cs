using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Students;

public class StudentProfileResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? StudentEmail { get; set; }

    public string? University { get; set; }

    public string? Major { get; set; }

    public int? GraduationYear { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int? CompletedJobs { get; set; }

    public decimal? TotalEarnings { get; set; }

    public string? Bio { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? CvUrl { get; set; }
}

public class StudentUpdateRequest
{
    public string? StudentEmail { get; set; }

    public string? University { get; set; }

    public string? Major { get; set; }

    public int? GraduationYear { get; set; }

    public string? Bio { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? CvUrl { get; set; }
}

public class StudentStatsResponse
{
    public int CompletedJobs { get; set; }

    public int ActiveApplications { get; set; }

    public int PendingApplications { get; set; }

    public decimal TotalEarnings { get; set; }

    public decimal AverageRating { get; set; }
}

public class StudentDashboardJobDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? CompanyName { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class StudentDashboardNotificationDto
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string Message { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class StudentWalletSummaryDto
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public decimal? Balance { get; set; }

    public decimal? TotalEarned { get; set; }

    public decimal? TotalWithdrawn { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class StudentDashboardResponse
{
    public StudentProfileResponse Student { get; set; } = new();

    public StudentWalletSummaryDto Wallet { get; set; } = new();

    public StudentStatsResponse Stats { get; set; } = new();

    public IReadOnlyList<StudentDashboardJobDto> RecentJobs { get; set; } = Array.Empty<StudentDashboardJobDto>();

    public IReadOnlyList<StudentDashboardNotificationDto> Notifications { get; set; } = Array.Empty<StudentDashboardNotificationDto>();
}
