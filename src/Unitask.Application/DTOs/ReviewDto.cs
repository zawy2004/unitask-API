using System;

namespace Unitask.Application.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid JobApplicationId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? SkillEndorsementsJson { get; set; }
    public bool? IsAnonymous { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

