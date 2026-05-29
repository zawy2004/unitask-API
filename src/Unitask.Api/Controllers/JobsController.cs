using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Jobs;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public JobsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobListItemResponse>>> GetJobs(
        [FromQuery] string? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isRemote,
        [FromQuery] bool? isFeatured,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        try
        {
            var jobQuery = _dbContext.Jobs.AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Business)
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            jobQuery = jobQuery.Where(j => j.Status == status);
        }

        if (categoryId.HasValue)
        {
            jobQuery = jobQuery.Where(j => j.CategoryId == categoryId.Value);
        }

        if (isRemote.HasValue)
        {
            jobQuery = jobQuery.Where(j => j.IsRemote == isRemote.Value);
        }

        if (isFeatured.HasValue)
        {
            jobQuery = jobQuery.Where(j => j.IsFeatured == isFeatured.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            jobQuery = jobQuery.Where(j =>
                j.Title.ToLower().Contains(term)
                || (j.Description != null && j.Description.ToLower().Contains(term))
                || (j.Location != null && j.Location.ToLower().Contains(term))
                || (j.Category != null && j.Category.Name.ToLower().Contains(term))
                || j.Business.CompanyName.ToLower().Contains(term)
                || (j.TagsJson != null && j.TagsJson.ToLower().Contains(term))
                || (j.RequiredSkillsJson != null && j.RequiredSkillsJson.ToLower().Contains(term)));
        }

            var total = await jobQuery.CountAsync();
            var items = await jobQuery
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(j => MapJob(j))
                .ToListAsync();

            return Ok(new PagedResult<JobListItemResponse>
            {
                Total = total,
                Page = page,
                Limit = limit,
                Data = items
            });
        }
        catch
        {
            return Ok(FallbackData.GetJobs(page, limit));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<JobListItemResponse>> CreateJob([FromBody] JobCreateRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var business = await _dbContext.BusinessProfiles
            .FirstOrDefaultAsync(b => b.UserId == userId.Value);
        if (business is null)
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.UserType == "business");
            if (user is null)
                return BadRequest(new { message = "Business profile not found." });

            business = new Unitask.Domain.Entities.BusinessProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                CompanyName = user.FullName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVerified = false,
                CompletedProjects = 0,
                TotalSpent = 0m,
                Rating = 0m
            };
            _dbContext.BusinessProfiles.Add(business);
            await _dbContext.SaveChangesAsync();
        }

        var job = new Job
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            TagsJson = SerializeJsonList(request.Tags),
            Status = "draft",
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Currency = request.Currency,
            DurationType = request.DurationType,
            DurationDays = request.DurationDays,
            RequiredSkillsJson = SerializeJsonList(request.RequiredSkills),
            ExperienceLevel = request.ExperienceLevel,
            SpotsTotal = request.SpotsTotal,
            SpotsFilled = 0,
            Location = request.Location,
            IsRemote = request.IsRemote,
            IsFeatured = false,
            Deadline = request.Deadline,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();

        var response = new JobListItemResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            CategoryId = job.CategoryId,
            BusinessId = job.BusinessId,
            CompanyName = business.CompanyName,
            Tags = ParseJsonList(job.TagsJson),
            Status = job.Status,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Currency = job.Currency,
            DurationType = job.DurationType,
            DurationDays = job.DurationDays,
            RequiredSkills = ParseJsonList(job.RequiredSkillsJson),
            ExperienceLevel = job.ExperienceLevel,
            SpotsTotal = job.SpotsTotal,
            SpotsFilled = job.SpotsFilled,
            Location = job.Location,
            IsRemote = job.IsRemote,
            IsFeatured = job.IsFeatured,
            Deadline = job.Deadline,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            PublishedAt = job.PublishedAt
        };

        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailsResponse>> GetJobById(Guid id)
    {
        try
        {
            var job = await _dbContext.Jobs.AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Business)
                .Include(j => j.JobApplications)
                .ThenInclude(a => a.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job is null)
            {
                var fallbackJob = FallbackData.GetJobById(id);
                return fallbackJob is null ? NotFound() : Ok(fallbackJob);
            }

            var studentUserIds = job.JobApplications
                .Select(a => a.Student.UserId)
                .Distinct()
                .ToList();

            var ratings = await _dbContext.Reviews.AsNoTracking()
                .Where(r => studentUserIds.Contains(r.ToUserId))
                .GroupBy(r => r.ToUserId)
                .Select(g => new { UserId = g.Key, Rating = g.Average(r => (decimal?)r.Rating) ?? 0m })
                .ToDictionaryAsync(x => x.UserId, x => x.Rating);

            var applicationResponses = job.JobApplications.Select(application => new JobApplicationSummaryDto
            {
                Id = application.Id,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                Student = new JobApplicationStudentDto
                {
                    Id = application.Student.Id,
                    Name = application.Student.User.FullName,
                    Rating = ratings.TryGetValue(application.Student.UserId, out var rating) ? rating : 0m
                }
            }).ToList();

            var response = new JobDetailsResponse
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Category = job.Category is null ? null : new JobCategoryInfoDto
                {
                    Id = job.Category.Id,
                    Name = job.Category.Name,
                    Slug = job.Category.Slug,
                    Description = job.Category.Description,
                    JobCount = job.Category.JobCount
                },
                Business = new JobBusinessInfoDto
                {
                    Id = job.Business.Id,
                    UserId = job.Business.UserId,
                    CompanyName = job.Business.CompanyName,
                    Rating = job.Business.Rating
                },
                Tags = ParseJsonList(job.TagsJson),
                Status = job.Status,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                RequiredSkills = ParseJsonList(job.RequiredSkillsJson),
                SpotsTotal = job.SpotsTotal,
                SpotsFilled = job.SpotsFilled,
                Location = job.Location,
                IsRemote = job.IsRemote,
                DurationType = job.DurationType,
                DurationDays = job.DurationDays,
                Deadline = job.Deadline,
                Applications = applicationResponses,
                CreatedAt = job.CreatedAt,
                PublishedAt = job.PublishedAt
            };

            return Ok(response);
        }
        catch
        {
            var fallbackJob = FallbackData.GetJobById(id);
            return fallbackJob is null ? NotFound() : Ok(fallbackJob);
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateJob(Guid id, [FromBody] JobUpdateRequest request)
    {
        var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            job.Title = request.Title;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            job.Description = request.Description;
        }

        if (request.CategoryId.HasValue)
        {
            job.CategoryId = request.CategoryId;
        }

        if (request.Tags is not null)
        {
            job.TagsJson = SerializeJsonList(request.Tags);
        }

        if (request.SalaryMin.HasValue)
        {
            job.SalaryMin = request.SalaryMin;
        }

        if (request.SalaryMax.HasValue)
        {
            job.SalaryMax = request.SalaryMax;
        }

        if (request.Currency is not null)
        {
            job.Currency = request.Currency;
        }

        if (request.DurationType is not null)
        {
            job.DurationType = request.DurationType;
        }

        if (request.DurationDays.HasValue)
        {
            job.DurationDays = request.DurationDays;
        }

        if (request.RequiredSkills is not null)
        {
            job.RequiredSkillsJson = SerializeJsonList(request.RequiredSkills);
        }

        if (request.ExperienceLevel is not null)
        {
            job.ExperienceLevel = request.ExperienceLevel;
        }

        if (request.SpotsTotal.HasValue)
        {
            job.SpotsTotal = request.SpotsTotal;
        }

        if (request.Location is not null)
        {
            job.Location = request.Location;
        }

        if (request.IsRemote.HasValue)
        {
            job.IsRemote = request.IsRemote;
        }

        if (request.IsFeatured.HasValue)
        {
            job.IsFeatured = request.IsFeatured;
        }

        if (request.Deadline.HasValue)
        {
            job.Deadline = request.Deadline;
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            job.Status = request.Status;
        }

        job.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null)
        {
            return NotFound();
        }

        _dbContext.Jobs.Remove(job);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:guid}/publish")]
    public async Task<IActionResult> PublishJob(Guid id)
    {
        var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null)
        {
            return NotFound();
        }

        job.Status = "open";
        job.PublishedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    private static JobListItemResponse MapJob(Job job)
    {
        return new JobListItemResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            CategoryId = job.CategoryId,
            CategoryName = job.Category != null ? job.Category.Name : null,
            BusinessId = job.BusinessId,
            CompanyName = job.Business.CompanyName,
            Tags = ParseJsonList(job.TagsJson),
            Status = job.Status,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Currency = job.Currency,
            DurationType = job.DurationType,
            DurationDays = job.DurationDays,
            RequiredSkills = ParseJsonList(job.RequiredSkillsJson),
            ExperienceLevel = job.ExperienceLevel,
            SpotsTotal = job.SpotsTotal,
            SpotsFilled = job.SpotsFilled,
            Location = job.Location,
            IsRemote = job.IsRemote,
            IsFeatured = job.IsFeatured,
            Deadline = job.Deadline,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            PublishedAt = job.PublishedAt
        };
    }

    private static int ScoreJob(Job job, string normalizedSearch)
    {
        if (string.IsNullOrWhiteSpace(normalizedSearch))
        {
            return 0;
        }

        var score = 0;
        var tokens = normalizedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var jobText = Normalize(string.Join(' ', new[]
        {
            job.Title,
            job.Description,
            job.Location,
            job.Category?.Name,
            job.Business.CompanyName,
            job.TagsJson,
            job.RequiredSkillsJson
        }.Where(value => !string.IsNullOrWhiteSpace(value))));

        score += tokens.Count(token => jobText.Contains(token)) * 5;

        if (job.IsFeatured == true)
        {
            score += 6;
        }

        if (job.Deadline.HasValue && job.Deadline.Value.Date <= DateTime.UtcNow.AddDays(7).Date)
        {
            score += 3;
        }

        return score;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    private static IReadOnlyList<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static string? SerializeJsonList(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(values);
    }
}

