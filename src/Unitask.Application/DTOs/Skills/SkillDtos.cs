using System;

namespace Unitask.Application.DTOs.Skills;

public class SkillResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public string? IconUrl { get; set; }
}

public class StudentSkillResponse
{
    public Guid Id { get; set; }

    public Guid SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Proficiency { get; set; }

    public int? EndorsementCount { get; set; }

    public DateTime? AddedAt { get; set; }
}

public class AddStudentSkillRequest
{
    public Guid SkillId { get; set; }

    public string? Proficiency { get; set; }
}

public class EndorseSkillRequest
{
    public Guid StudentId { get; set; }
}
