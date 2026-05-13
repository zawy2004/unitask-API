using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Applications;
using Unitask.Application.DTOs.Common;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api")]
public class JobApplicationsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public JobApplicationsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
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
        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(a => new JobApplicationListItemResponse
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
                    Id = a.Student.Id,
                    Name = a.Student.User.FullName,
                    University = a.Student.University,
                    Rating = a.Student.User.ReviewToUsers.Any()
                        ? a.Student.User.ReviewToUsers.Average(r => (decimal?)r.Rating)
                        : 0m,
                    CompletedJobs = a.Student.CompletedJobs
                }
            })
            .ToListAsync();

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

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
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
                Name = application.Student.User.FullName,
                University = application.Student.University,
                Rating = application.Student.User.ReviewToUsers.Any()
                    ? application.Student.User.ReviewToUsers.Average(r => (decimal?)r.Rating)
                    : 0m,
                CompletedJobs = application.Student.CompletedJobs
            }
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPut("applications/{id:guid}/accept")]
    public async Task<IActionResult> AcceptApplication(Guid id, [FromBody] JobApplicationAcceptRequest request)
    {
        var application = await _dbContext.JobApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (application is null)
        {
            return NotFound();
        }

        application.Status = "accepted";
        application.AcceptedAt = DateTime.UtcNow;
        application.StartedAt = request.StartDate;
        await _dbContext.SaveChangesAsync();

        return NoContent();
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

