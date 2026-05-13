using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("StudentId", Name = "UQ__StudentW__32C52B980C342C3B", IsUnique = true)]
[Index("StudentId", Name = "idx_wallet_student")]
public partial class StudentWallet
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? Balance { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalEarned { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalWithdrawn { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentWallet")]
    public virtual StudentProfile Student { get; set; } = null!;
}
