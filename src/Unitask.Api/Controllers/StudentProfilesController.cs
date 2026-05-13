using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
        {
            return NotFound();
        }

        return Ok(MapStudent(student));
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateStudent(Guid userId, [FromBody] StudentUpdateRequest request)
    {
        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
        {
            return NotFound();
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

    [HttpPost("{userId:guid}/verify")]
    public async Task<IActionResult> VerifyStudent(Guid userId, [FromForm] string studentEmail, [FromForm] IFormFile? studentIdCard)
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

    [HttpGet("{userId:guid}/dashboard")]
    public async Task<ActionResult<StudentDashboardResponse>> GetDashboard(Guid userId)
    {
        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student is null)
        {
            return NotFound();
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
            CvUrl = student.CvUrl
        };
    }
}

