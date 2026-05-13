using System;

namespace Unitask.Application.DTOs;

public class JobCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int? JobCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

