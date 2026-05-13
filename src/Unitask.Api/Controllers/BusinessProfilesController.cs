using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Businesses;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public BusinessesController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<BusinessProfileResponse>> GetBusiness(Guid userId)
    {
        var business = await _dbContext.BusinessProfiles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (business is null)
        {
            return NotFound();
        }

        return Ok(MapBusiness(business));
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateBusiness(Guid userId, [FromBody] BusinessUpdateRequest request)
    {
        var business = await _dbContext.BusinessProfiles
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (business is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            business.CompanyName = request.CompanyName;
        }

        if (request.CompanyEmail is not null)
        {
            business.CompanyEmail = request.CompanyEmail;
        }

        if (request.CompanyWebsite is not null)
        {
            business.CompanyWebsite = request.CompanyWebsite;
        }

        if (request.CompanySize is not null)
        {
            business.CompanySize = request.CompanySize;
        }

        if (request.Industry is not null)
        {
            business.Industry = request.Industry;
        }

        if (request.Description is not null)
        {
            business.Description = request.Description;
        }

        if (request.LogoUrl is not null)
        {
            business.LogoUrl = request.LogoUrl;
        }

        if (request.Address is not null)
        {
            business.Address = request.Address;
        }

        business.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{userId:guid}/dashboard")]
    public async Task<ActionResult<BusinessDashboardResponse>> GetDashboard(Guid userId)
    {
        var business = await _dbContext.BusinessProfiles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId);
        if (business is null)
        {
            return NotFound();
        }

        var openJobsCount = await _dbContext.Jobs.AsNoTracking()
            .CountAsync(j => j.BusinessId == business.Id && j.Status == "open");

        var totalApplications = await _dbContext.JobApplications.AsNoTracking()
            .CountAsync(a => a.Job.BusinessId == business.Id);

        var pendingApplications = await _dbContext.JobApplications.AsNoTracking()
            .CountAsync(a => a.Job.BusinessId == business.Id && a.Status == "pending");

        var recentApplications = await _dbContext.JobApplications.AsNoTracking()
            .Where(a => a.Job.BusinessId == business.Id)
            .OrderByDescending(a => a.AppliedAt)
            .Include(a => a.Job)
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .Select(a => new BusinessDashboardApplicationDto
            {
                Id = a.Id,
                Status = a.Status,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                StudentName = a.Student.User.FullName,
                AppliedAt = a.AppliedAt
            })
            .Take(5)
            .ToListAsync();

        var openJobs = await _dbContext.Jobs.AsNoTracking()
            .Where(j => j.BusinessId == business.Id && j.Status == "open")
            .Select(j => new BusinessDashboardJobDto
            {
                Id = j.Id,
                Title = j.Title,
                Status = j.Status,
                SpotsFilled = j.SpotsFilled,
                SpotsTotal = j.SpotsTotal
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

        var response = new BusinessDashboardResponse
        {
            Business = MapBusiness(business),
            Stats = new BusinessStatsResponse
            {
                OpenJobs = openJobsCount,
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                TotalSpent = business.TotalSpent ?? 0m,
                CompletedProjects = business.CompletedProjects ?? 0,
                AverageRating = business.Rating ?? 0m
            },
            RecentApplications = recentApplications,
            OpenJobs = openJobs,
            Notifications = notifications
        };

        return Ok(response);
    }

    private static BusinessProfileResponse MapBusiness(Unitask.Domain.Entities.BusinessProfile business)
    {
        return new BusinessProfileResponse
        {
            Id = business.Id,
            UserId = business.UserId,
            CompanyName = business.CompanyName,
            CompanyEmail = business.CompanyEmail,
            CompanyWebsite = business.CompanyWebsite,
            CompanySize = business.CompanySize,
            Industry = business.Industry,
            IsVerified = business.IsVerified,
            VerifiedAt = business.VerifiedAt,
            CompletedProjects = business.CompletedProjects,
            TotalSpent = business.TotalSpent,
            Rating = business.Rating,
            Description = business.Description,
            LogoUrl = business.LogoUrl,
            Address = business.Address
        };
    }
}

