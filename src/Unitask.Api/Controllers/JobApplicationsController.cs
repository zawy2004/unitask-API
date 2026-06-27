using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unitask.Api.Extensions;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Settings;
using Unitask.Application.DTOs.Applications;
using Unitask.Application.DTOs.Common;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api")]
public class JobApplicationsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<JobApplicationsController> _logger;

    public JobApplicationsController(
        UnitaskDbContext dbContext,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<JobApplicationsController> logger)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    [HttpGet("jobs/{jobId:guid}/applications")]
    public async Task<ActionResult<PagedResult<JobApplicationListItemResponse>>> GetApplicationsForJob(
        Guid jobId,
        [FromQuery] string? status,
        [FromQuery] int page = 1)
    {
        const int limit = 10;
        var query = _dbContext.JobApplications.AsNoTracking()
            .Where(a => a.JobId == jobId)
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var total = await query.CountAsync();
        var pagedApps = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.JobId,
                a.StudentId,
                a.Status,
                a.CoverLetter,
                a.ProposedTimeline,
                a.AppliedAt,
                StudentProfileId = a.Student.Id,
                StudentName = a.Student.User.FullName,
                a.Student.University,
                a.Student.CompletedJobs,
                StudentUserId = a.Student.UserId
            })
            .ToListAsync();

        var studentUserIds = pagedApps.Select(a => a.StudentUserId).Distinct().ToList();
        var ratings = await _dbContext.Reviews.AsNoTracking()
            .Where(r => studentUserIds.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new { UserId = g.Key, Rating = g.Average(r => (decimal?)r.Rating) ?? 0m })
            .ToDictionaryAsync(x => x.UserId, x => x.Rating);

        var items = pagedApps.Select(a => new JobApplicationListItemResponse
        {
            Id = a.Id,
            JobId = a.JobId,
            StudentId = a.StudentId,
            Status = a.Status,
            CoverLetter = a.CoverLetter,
            ProposedTimeline = a.ProposedTimeline,
            AppliedAt = a.AppliedAt,
            Student = new ApplicationStudentDto
            {
                Id = a.StudentProfileId,
                UserId = a.StudentUserId,
                Name = a.StudentName,
                University = a.University,
                Rating = ratings.TryGetValue(a.StudentUserId, out var r) ? r : 0m,
                CompletedJobs = a.CompletedJobs
            }
        }).ToList();

        return Ok(new PagedResult<JobApplicationListItemResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [Authorize]
    [HttpPost("jobs/{jobId:guid}/apply")]
    public async Task<ActionResult<JobApplicationResponse>> ApplyToJob(Guid jobId, [FromBody] ApplyJobRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        // Khung M1–M3: chặn nhận task nếu tài khoản bị khóa (M3) hoặc đang đình chỉ (M2).
        var actor = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (actor is not null && (actor.IsActive == false || (actor.SuspendedUntil != null && actor.SuspendedUntil > DateTime.UtcNow)))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản đang bị đình chỉ/khóa, không thể nhận task." });

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.UserType == "student");
            if (user is null)
                return BadRequest(new { message = "Student profile not found." });

            student = new Unitask.Domain.Entities.StudentProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                StudentEmail = user.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVerified = false,
                CompletedJobs = 0,
                TotalEarnings = 0m
            };
            _dbContext.StudentProfiles.Add(student);
            await _dbContext.SaveChangesAsync();
        }

        var exists = await _dbContext.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.StudentId == student.Id);
        if (exists)
        {
            return Conflict(new { message = "Already applied to this job." });
        }

        var application = new Unitask.Domain.Entities.JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            StudentId = student.Id,
            Status = "pending",
            AppliedAt = DateTime.UtcNow,
            CoverLetter = request.CoverLetter,
            ProposedTimeline = request.ProposedTimeline
        };

        _dbContext.JobApplications.Add(application);
        await _dbContext.SaveChangesAsync();

        return Ok(new JobApplicationResponse
        {
            Id = application.Id,
            JobId = application.JobId,
            StudentId = application.StudentId,
            Status = application.Status,
            AppliedAt = application.AppliedAt
        });
    }

    [HttpGet("applications/{id:guid}")]
    public async Task<ActionResult<JobApplicationListItemResponse>> GetApplication(Guid id)
    {
        var application = await _dbContext.JobApplications.AsNoTracking()
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        var avgRating = await _dbContext.Reviews.AsNoTracking()
            .Where(r => r.ToUserId == application.Student.UserId)
            .AverageAsync(r => (decimal?)r.Rating) ?? 0m;

        var response = new JobApplicationListItemResponse
        {
            Id = application.Id,
            JobId = application.JobId,
            StudentId = application.StudentId,
            Status = application.Status,
            CoverLetter = application.CoverLetter,
            ProposedTimeline = application.ProposedTimeline,
            AppliedAt = application.AppliedAt,
            Student = new ApplicationStudentDto
            {
                Id = application.Student.Id,
                UserId = application.Student.UserId,
                Name = application.Student.User.FullName,
                University = application.Student.University,
                Rating = avgRating,
                CompletedJobs = application.Student.CompletedJobs
            }
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPut("applications/{id:guid}/accept")]
    public async Task<IActionResult> AcceptApplication(Guid id, [FromBody] JobApplicationAcceptRequest request)
    {
        var application = await _dbContext.JobApplications
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.Job).ThenInclude(j => j.Business)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = "accepted";
        application.AcceptedAt = DateTime.UtcNow;
        application.StartedAt = request.StartDate;
        await _dbContext.SaveChangesAsync();

        // Thông báo trong app cho sinh viên.
        _dbContext.Notifications.Add(new Unitask.Domain.Entities.Notification
        {
            Id = Guid.NewGuid(),
            UserId = application.Student.UserId,
            Type = "application_accepted",
            Title = "Bạn đã được nhận vào dự án",
            Message = $"Doanh nghiệp {application.Job.Business.CompanyName} đã chấp nhận bạn cho dự án \"{application.Job.Title}\".",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        });
        await _dbContext.SaveChangesAsync();

        // Gửi email "Nhận được Offer" cho sinh viên (lỗi gửi mail không làm hỏng việc nhận).
        try
        {
            await _emailService.SendTemplateAsync(
                EmailTemplate.OfferReceived,
                application.Student.User.Email,
                new Dictionary<string, string>
                {
                    ["studentName"] = application.Student.User.FullName,
                    ["businessName"] = application.Job.Business.CompanyName,
                    ["projectName"] = application.Job.Title,
                    ["offerAmount"] = FormatSalary(application.Job.SalaryMin, application.Job.SalaryMax),
                    ["duration"] = FormatDuration(application.Job.DurationDays, application.Job.DurationType),
                    ["startDate"] = request.StartDate?.ToString("dd/MM/yyyy") ?? "Theo thỏa thuận",
                    ["viewOfferUrl"] = $"{_emailSettings.FrontendBaseUrl}/my-applications",
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không gửi được email Offer cho sinh viên (application {Id})", id);
        }

        return NoContent();
    }

    /// <summary>Định dạng mức thù lao (VND) từ khoảng lương của job.</summary>
    private static string FormatSalary(decimal? min, decimal? max)
    {
        string Money(decimal v) => v.ToString("#,0", CultureInfo.GetCultureInfo("vi-VN")) + "đ";
        if (min.HasValue && max.HasValue && min.Value != max.Value) return $"{Money(min.Value)} - {Money(max.Value)}";
        if (max.HasValue) return Money(max.Value);
        if (min.HasValue) return Money(min.Value);
        return "Thỏa thuận";
    }

    /// <summary>Định dạng thời hạn dự án.</summary>
    private static string FormatDuration(int? days, string? type)
    {
        if (days.HasValue && days.Value > 0) return $"{days.Value} ngày";
        return string.IsNullOrWhiteSpace(type) ? "Theo thỏa thuận" : type!;
    }

    [Authorize]
    [HttpPut("applications/{id:guid}/reject")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] JobApplicationRejectRequest request)
    {
        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = "rejected";
        application.RejectedAt = DateTime.UtcNow;
        application.RejectionReason = request.RejectionReason;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPut("applications/{id:guid}/complete")]
    public async Task<IActionResult> CompleteApplication(Guid id)
    {
        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = "completed";
        application.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpGet("my-applications")]
    public async Task<ActionResult<IReadOnlyList<MyApplicationResponse>>> GetMyApplications()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
        }

        var applications = await _dbContext.JobApplications.AsNoTracking()
            .Where(a => a.StudentId == student.Id)
            .Include(a => a.Job)
            .ThenInclude(j => j.Business)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new MyApplicationResponse
            {
                Id = a.Id,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                AcceptedAt = a.AcceptedAt,
                Job = new MyApplicationJobDto
                {
                    Id = a.Job.Id,
                    Title = a.Job.Title,
                    Company = a.Job.Business.CompanyName,
                    Salary = FormatSalary(a.Job.SalaryMin, a.Job.SalaryMax, a.Job.Currency)
                }
            })
            .ToListAsync();

        return Ok(applications);
    }

    [Authorize]
    [HttpGet("my-business-applicants")]
    public async Task<ActionResult<IReadOnlyList<JobApplicationListItemResponse>>> GetBusinessApplicants()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var business = await _dbContext.BusinessProfiles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId.Value);
        if (business is null)
        {
            return BadRequest(new { message = "Business profile not found." });
        }

        var apps = await _dbContext.JobApplications.AsNoTracking()
            .Where(a => a.Job.BusinessId == business.Id)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new
            {
                a.Id,
                a.JobId,
                a.StudentId,
                a.Status,
                a.CoverLetter,
                a.ProposedTimeline,
                a.AppliedAt,
                StudentProfileId = a.Student.Id,
                StudentName = a.Student.User.FullName,
                a.Student.University,
                a.Student.CompletedJobs,
                StudentUserId = a.Student.UserId
            })
            .ToListAsync();

        var studentUserIds = apps.Select(a => a.StudentUserId).Distinct().ToList();
        var ratings = await _dbContext.Reviews.AsNoTracking()
            .Where(r => studentUserIds.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new { UserId = g.Key, Rating = g.Average(r => (decimal?)r.Rating) ?? 0m })
            .ToDictionaryAsync(x => x.UserId, x => x.Rating);

        var result = apps.Select(a => new JobApplicationListItemResponse
        {
            Id = a.Id,
            JobId = a.JobId,
            StudentId = a.StudentId,
            Status = a.Status,
            CoverLetter = a.CoverLetter,
            ProposedTimeline = a.ProposedTimeline,
            AppliedAt = a.AppliedAt,
            Student = new ApplicationStudentDto
            {
                Id = a.StudentProfileId,
                UserId = a.StudentUserId,
                Name = a.StudentName,
                University = a.University,
                Rating = ratings.TryGetValue(a.StudentUserId, out var r) ? r : 0m,
                CompletedJobs = a.CompletedJobs
            }
        }).ToList();

        return Ok(result);
    }

    private static string? FormatSalary(decimal? min, decimal? max, string? currency)
    {
        if (!min.HasValue && !max.HasValue)
        {
            return null;
        }

        var symbol = string.IsNullOrWhiteSpace(currency) ? string.Empty : $" {currency}";
        if (min.HasValue && max.HasValue)
        {
            return $"{min:0.##} - {max:0.##}{symbol}";
        }

        if (min.HasValue)
        {
            return $"From {min:0.##}{symbol}";
        }

        return $"Up to {max:0.##}{symbol}";
    }
}

