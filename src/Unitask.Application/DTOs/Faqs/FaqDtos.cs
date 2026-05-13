using System;

namespace Unitask.Application.DTOs.Faqs;

public class FaqResponse
{
    public Guid Id { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public string? Category { get; set; }

    public int? ViewCount { get; set; }
}
