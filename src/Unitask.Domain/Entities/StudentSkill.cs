using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("StudentId", "SkillId", Name = "UQ__StudentS__5F3F22803C6777EE", IsUnique = true)]
[Index("SkillId", Name = "idx_student_skill_skill")]
[Index("StudentId", Name = "idx_student_skill_student")]
public partial class StudentSkill
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid SkillId { get; set; }

    [StringLength(50)]
    public string? Proficiency { get; set; }

    public int? EndorsementCount { get; set; }

    public DateTime? AddedAt { get; set; }

    [ForeignKey("SkillId")]
    [InverseProperty("StudentSkills")]
    public virtual Skill Skill { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("StudentSkills")]
    public virtual StudentProfile Student { get; set; } = null!;
}
