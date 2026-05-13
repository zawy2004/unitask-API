using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Blog;

public class BlogPostListItemResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? FeaturedImageUrl { get; set; }

    public string? Category { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}

public class BlogPostResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? FeaturedImageUrl { get; set; }

    public string? Category { get; set; }

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public string? AuthorName { get; set; }
}

public class BlogPostCreateRequest
{
    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? FeaturedImageUrl { get; set; }

    public string? Category { get; set; }

    public IReadOnlyList<string>? Tags { get; set; }

    public string? Status { get; set; }
}
