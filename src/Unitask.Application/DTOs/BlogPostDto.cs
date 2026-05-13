using System;

namespace Unitask.Application.DTOs;

public class BlogPostDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? Category { get; set; }
    public string? TagsJson { get; set; }
    public string? Status { get; set; }
    public int? ViewCount { get; set; }
    public int? LikeCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

