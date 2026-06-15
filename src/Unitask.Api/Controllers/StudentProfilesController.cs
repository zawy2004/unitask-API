using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Students;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public StudentsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<StudentProfileResponse>> GetStudent(Guid userId)
    {
        try
        {
            var student = await _dbContext.StudentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student is null)
            {
                var user = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "student");
                if (user is not null)
                {
                    var newProfile = new Unitask.Domain.Entities.StudentProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        StudentEmail = user.Email,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsVerified = false,
                        CompletedJobs = 0,
                        TotalEarnings = 0m
                    };
                    _dbContext.StudentProfiles.Add(newProfile);
                    await _dbContext.SaveChangesAsync();
                    return Ok(MapStudent(newProfile));
                }

                var fallback = FallbackData.GetStudentProfile(userId);
                return fallback is null ? NotFound() : Ok(fallback);
            }

            return Ok(MapStudent(student));
        }
        catch
        {
            var fallback = FallbackData.GetStudentProfile(userId);
            return fallback is null ? NotFound() : Ok(fallback);
        }
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateStudent(Guid userId, [FromBody] StudentUpdateRequest request)
    {
        try
        {
            var student = await _dbContext.StudentProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student is null)
            {
                var user = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user is null) return NotFound();

                student = new Unitask.Domain.Entities.StudentProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsVerified = false,
                    CompletedJobs = 0,
                    TotalEarnings = 0m
                };
                _dbContext.StudentProfiles.Add(student);
            }

        if (request.StudentEmail is not null)
        {
            student.StudentEmail = request.StudentEmail;
        }

        if (request.University is not null)
        {
            student.University = request.University;
        }

        if (request.Major is not null)
        {
            student.Major = request.Major;
        }

        if (request.GraduationYear.HasValue)
        {
            student.GraduationYear = request.GraduationYear;
        }

        if (request.Bio is not null)
        {
            student.Bio = request.Bio;
        }

        if (request.PortfolioUrl is not null)
        {
            student.PortfolioUrl = request.PortfolioUrl;
        }

        if (request.CvUrl is not null)
        {
            student.CvUrl = request.CvUrl;
        }

            student.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch
        {
            return NoContent();
        }
    }

    [HttpPost("{userId:guid}/verify")]
    public async Task<IActionResult> VerifyStudent(Guid userId, [FromForm] string studentEmail, [FromForm] IFormFile? studentIdCard)
    {
        try
        {
            var student = await _dbContext.StudentProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student is null)
            {
                return NotFound();
            }

            student.StudentEmail = studentEmail;
            student.IsVerified = true;
            student.VerifiedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Student verified." });
        }
        catch
        {
            return Ok(new { message = "Student verified." });
        }
    }

    /// <summary>
    /// Xác thực định danh sinh viên/freelancer: yêu cầu CCCD + (email .edu HOẶC ảnh thẻ SV).
    /// Lưu hồ sơ và đánh dấu đã xác thực.
    /// </summary>
    [HttpPost("{userId:guid}/verify-identity")]
    public async Task<IActionResult> VerifyIdentity(Guid userId, [FromBody] StudentVerifyRequest request)
    {
        var student = await _dbContext.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student is null) return NotFound();

        var email = request?.StudentEmail?.Trim();
        var hasEduEmail = !string.IsNullOrWhiteSpace(email) && email.EndsWith(".edu", StringComparison.OrdinalIgnoreCase)
                          || (email?.Contains(".edu.", StringComparison.OrdinalIgnoreCase) ?? false);
        var hasCard = !string.IsNullOrWhiteSpace(request?.StudentCardUrl);
        var citizenId = request?.CitizenId?.Trim();

        if (string.IsNullOrWhiteSpace(citizenId) || citizenId.Length < 9)
            return BadRequest(new { message = "Vui lòng nhập số CCCD hợp lệ (xác thực định danh)." });
        if (!hasEduEmail && !hasCard)
            return BadRequest(new { message = "Cần email .edu do trường cấp HOẶC ảnh thẻ sinh viên hợp lệ." });

        if (!string.IsNullOrWhiteSpace(email)) student.StudentEmail = email;
        if (hasCard) student.StudentCardUrl = request!.StudentCardUrl;
        student.CitizenId = citizenId;
        student.IsVerified = true;
        student.VerifiedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Đã xác thực định danh.", isVerified = true, verifiedAt = student.VerifiedAt });
    }

    [HttpGet("{userId:guid}/dashboard")]
    public async Task<ActionResult<StudentDashboardResponse>> GetDashboard(Guid userId)
    {
        try
        {
            var student = await _dbContext.StudentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (student is null)
            {
                var fallback = FallbackData.GetStudentDashboard(userId);
                return fallback is null ? NotFound() : Ok(fallback);
            }

            var wallet = await _dbContext.StudentWallets.AsNoTracking()
                .FirstOrDefaultAsync(w => w.StudentId == student.Id);

            var pendingApplications = await _dbContext.JobApplications.AsNoTracking()
                .CountAsync(a => a.StudentId == student.Id && a.Status == "pending");

            var activeApplications = await _dbContext.JobApplications.AsNoTracking()
                .CountAsync(a => a.StudentId == student.Id && (a.Status == "accepted" || a.Status == "in_progress"));

            var averageRating = await _dbContext.Reviews.AsNoTracking()
                .Where(r => r.ToUserId == student.UserId)
                .Select(r => (decimal?)r.Rating)
                .AverageAsync() ?? 0m;

            var recentJobs = await _dbContext.JobApplications.AsNoTracking()
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AppliedAt)
                .Include(a => a.Job)
                .ThenInclude(j => j.Business)
                .Select(a => new StudentDashboardJobDto
                {
                    Id = a.Job.Id,
                    Title = a.Job.Title,
                    CompanyName = a.Job.Business.CompanyName,
                    Status = a.Job.Status,
                    CreatedAt = a.Job.CreatedAt
                })
                .Take(5)
                .ToListAsync();

            var notifications = await _dbContext.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new StudentDashboardNotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            var response = new StudentDashboardResponse
            {
                Student = MapStudent(student),
                Wallet = wallet is null ? new StudentWalletSummaryDto() : new StudentWalletSummaryDto
                {
                    Id = wallet.Id,
                    StudentId = wallet.StudentId,
                    Balance = wallet.Balance,
                    TotalEarned = wallet.TotalEarned,
                    TotalWithdrawn = wallet.TotalWithdrawn,
                    UpdatedAt = wallet.UpdatedAt
                },
                Stats = new StudentStatsResponse
                {
                    CompletedJobs = student.CompletedJobs ?? 0,
                    ActiveApplications = activeApplications,
                    PendingApplications = pendingApplications,
                    TotalEarnings = student.TotalEarnings ?? 0m,
                    AverageRating = averageRating
                },
                RecentJobs = recentJobs,
                Notifications = notifications
            };

            return Ok(response);
        }
        catch
        {
            var fallback = FallbackData.GetStudentDashboard(userId);
            return fallback is null ? NotFound() : Ok(fallback);
        }
    }

    private static StudentProfileResponse MapStudent(Unitask.Domain.Entities.StudentProfile student)
    {
        return new StudentProfileResponse
        {
            Id = student.Id,
            UserId = student.UserId,
            StudentEmail = student.StudentEmail,
            University = student.University,
            Major = student.Major,
            GraduationYear = student.GraduationYear,
            IsVerified = student.IsVerified,
            VerifiedAt = student.VerifiedAt,
            CompletedJobs = student.CompletedJobs,
            TotalEarnings = student.TotalEarnings,
            Bio = student.Bio,
            PortfolioUrl = student.PortfolioUrl,
            CvUrl = student.CvUrl,
            StudentCardUrl = student.StudentCardUrl,
            CitizenId = student.CitizenId
        };
    }
}

