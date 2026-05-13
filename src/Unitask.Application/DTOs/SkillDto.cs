using System;

namespace Unitask.Application.DTOs;

public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public string? IconUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}

