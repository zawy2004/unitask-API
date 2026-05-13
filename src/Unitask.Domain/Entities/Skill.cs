using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Name", Name = "UQ__Skills__737584F68FA901A4", IsUnique = true)]
[Index("Category", Name = "idx_skill_category")]
[Index("Name", Name = "idx_skill_name")]
public partial class Skill
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string? Category { get; set; }

    public string? IconUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Skill")]
    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
