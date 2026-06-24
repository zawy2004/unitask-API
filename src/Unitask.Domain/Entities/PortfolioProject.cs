using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("StudentId", Name = "idx_portfolio_student")]
[Index("StudentId", "SortOrder", Name = "idx_portfolio_student_order")]
public class PortfolioProject
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? ProjectUrl { get; set; }

    [StringLength(500)]
    public string? GithubUrl { get; set; }

    [StringLength(1000)]
    public string? Tags { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsHighlighted { get; set; }

    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("PortfolioProjects")]
    public virtual StudentProfile Student { get; set; } = null!;
}
