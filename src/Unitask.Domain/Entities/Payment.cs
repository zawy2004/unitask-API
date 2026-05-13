using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("JobApplicationId", Name = "idx_payment_application")]
[Index("CreatedAt", Name = "idx_payment_created_at")]
[Index("Status", Name = "idx_payment_status")]
public partial class Payment
{
    [Key]
    public Guid Id { get; set; }

    public Guid? JobId { get; set; }

    public Guid JobApplicationId { get; set; }

    public Guid? BusinessId { get; set; }

    public Guid? StudentId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Amount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    [ForeignKey("BusinessId")]
    [InverseProperty("Payments")]
    public virtual BusinessProfile? Business { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("Payments")]
    public virtual Job? Job { get; set; }

    [ForeignKey("JobApplicationId")]
    [InverseProperty("Payments")]
    public virtual JobApplication JobApplication { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Payments")]
    public virtual StudentProfile? Student { get; set; }
}
