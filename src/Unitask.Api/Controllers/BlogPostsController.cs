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
using Unitask.Application.DTOs.Blog;
using Unitask.Application.DTOs.Common;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogPostsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public BlogPostsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("posts")]
    public async Task<ActionResult<PagedResult<BlogPostListItemResponse>>> GetPosts(
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] int page = 1)
    {
        const int limit = 10;
        try
        {
            var query = _dbContext.BlogPosts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new BlogPostListItemResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Excerpt = p.Excerpt,
                    FeaturedImageUrl = p.FeaturedImageUrl,
                    Category = p.Category,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    PublishedAt = p.PublishedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<BlogPostListItemResponse>
            {
                Total = total,
                Page = page,
                Limit = limit,
                Data = items
            });
        }
        catch
        {
            return Ok(FallbackData.GetBlogPosts(page, limit));
        }
    }

    [HttpGet("posts/{slug}")]
    public async Task<ActionResult<BlogPostResponse>> GetPostBySlug(string slug)
    {
        try
        {
            var post = await _dbContext.BlogPosts.AsNoTracking()
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post is null)
            {
                var fallbackPost = FallbackData.GetBlogPostBySlug(slug);
                return fallbackPost is null ? NotFound() : Ok(fallbackPost);
            }

            var response = new BlogPostResponse
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                Excerpt = post.Excerpt,
                FeaturedImageUrl = post.FeaturedImageUrl,
                Category = post.Category,
                Tags = ParseJsonList(post.TagsJson),
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                AuthorName = post.Author.FullName
            };

            return Ok(response);
        }
        catch
        {
            var fallbackPost = FallbackData.GetBlogPostBySlug(slug);
            return fallbackPost is null ? NotFound() : Ok(fallbackPost);
        }
    }

    [Authorize]
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] BlogPostCreateRequest request)
    {
        var userId = User.GetUserId();
        var role = User.GetUserRole();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var post = new Unitask.Domain.Entities.BlogPost
        {
            Id = Guid.NewGuid(),
            AuthorId = userId.Value,
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            Excerpt = request.Excerpt,
            FeaturedImageUrl = request.FeaturedImageUrl,
            Category = request.Category,
            TagsJson = SerializeJsonList(request.Tags),
            Status = request.Status ?? "draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = request.Status == "published" ? DateTime.UtcNow : null
        };

        _dbContext.BlogPosts.Add(post);
        await _dbContext.SaveChangesAsync();

        return Ok();
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

