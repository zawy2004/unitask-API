using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("StudentId", Name = "idx_certification_student")]
public class Certification
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? IssuingOrganization { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [StringLength(500)]
    public string? CredentialUrl { get; set; }

    [StringLength(100)]
    public string? CredentialId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Certifications")]
    public virtual StudentProfile Student { get; set; } = null!;
}
