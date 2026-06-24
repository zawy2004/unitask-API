using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("StudentId", Name = "idx_education_student")]
public class Education
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    [StringLength(255)]
    public string Institution { get; set; } = null!;

    [StringLength(255)]
    public string? Degree { get; set; }

    [StringLength(255)]
    public string? FieldOfStudy { get; set; }

    public int? StartYear { get; set; }

    public int? EndYear { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? Gpa { get; set; }

    public string? Description { get; set; }

    public bool? IsCurrent { get; set; }

    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Educations")]
    public virtual StudentProfile Student { get; set; } = null!;
}
