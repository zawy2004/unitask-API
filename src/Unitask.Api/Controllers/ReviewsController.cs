using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Reviews;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public ReviewsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("reviews")]
    public async Task<ActionResult<PagedResult<ReviewListItemResponse>>> GetReviews(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? jobId,
        [FromQuery] int page = 1)
    {
        const int limit = 10;
        var query = _dbContext.Reviews.AsNoTracking().AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(r => r.ToUserId == userId.Value);
        }

        if (jobId.HasValue)
        {
            query = query.Where(r => r.JobId == jobId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => new ReviewListItemResponse
            {
                Id = r.Id,
                JobId = r.JobId,
                ApplicationId = r.JobApplicationId,
                Rating = r.Rating,
                Comment = r.Comment,
                SkillEndorsements = ParseJsonList(r.SkillEndorsementsJson),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResult<ReviewListItemResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [Authorize]
    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] ReviewCreateRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var application = await _dbContext.JobApplications
            .Include(a => a.Job)
            .ThenInclude(j => j.Business)
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.Id == request.JobApplicationId);
        if (application is null)
        {
            return BadRequest(new { message = "Job application not found." });
        }

        var toUserId = application.Student.UserId;
        if (application.Job.Business.UserId == userId.Value)
        {
            toUserId = application.Student.UserId;
        }
        else
        {
            toUserId = application.Job.Business.UserId;
        }

        var review = new Unitask.Domain.Entities.Review
        {
            Id = Guid.NewGuid(),
            JobId = application.JobId,
            JobApplicationId = application.Id,
            FromUserId = userId.Value,
            ToUserId = toUserId,
            Rating = request.Rating,
            Comment = request.Comment,
            SkillEndorsementsJson = SerializeJsonList(request.SkillEndorsements),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("users/{userId:guid}/reviews")]
    public async Task<ActionResult<PagedResult<ReviewListItemResponse>>> GetReviewsForUser(Guid userId, [FromQuery] int page = 1)
    {
        return await GetReviews(userId, null, page);
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

