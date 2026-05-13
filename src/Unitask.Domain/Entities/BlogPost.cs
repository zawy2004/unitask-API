using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Slug", Name = "UQ__BlogPost__BC7B5FB6BEECFCA5", IsUnique = true)]
[Index("PublishedAt", Name = "idx_blog_published_at")]
[Index("Slug", Name = "idx_blog_slug")]
[Index("Status", Name = "idx_blog_status")]
public partial class BlogPost
{
    [Key]
    public Guid Id { get; set; }

    public Guid AuthorId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string? FeaturedImageUrl { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public string? TagsJson { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public int? ViewCount { get; set; }

    public int? LikeCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("BlogPosts")]
    public virtual User Author { get; set; } = null!;
}
