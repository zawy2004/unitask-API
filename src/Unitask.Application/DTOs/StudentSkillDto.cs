using System;

namespace Unitask.Application.DTOs;

public class StudentSkillDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid SkillId { get; set; }
    public string? Proficiency { get; set; }
    public int? EndorsementCount { get; set; }
    public DateTime? AddedAt { get; set; }
}

