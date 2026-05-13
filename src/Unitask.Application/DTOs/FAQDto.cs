using System;

namespace Unitask.Application.DTOs;

public class FAQDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public string? Category { get; set; }
    public int? ViewCount { get; set; }
    public int? HelpfulCount { get; set; }
    public int? OrderIndex { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

