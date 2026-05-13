using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Reviews;

public class ReviewListItemResponse
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid ApplicationId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public IReadOnlyList<string> SkillEndorsements { get; set; } = Array.Empty<string>();

    public DateTime? CreatedAt { get; set; }
}

public class ReviewCreateRequest
{
    public Guid JobApplicationId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public IReadOnlyList<string>? SkillEndorsements { get; set; }
}
