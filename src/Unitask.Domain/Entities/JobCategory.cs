using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Slug", Name = "UQ__JobCateg__BC7B5FB617A64871", IsUnique = true)]
[Index("Slug", Name = "idx_category_slug")]
public partial class JobCategory
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public int? JobCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
