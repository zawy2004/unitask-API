using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Status", Name = "idx_withdrawal_status")]
[Index("StudentId", Name = "idx_withdrawal_student")]
public partial class WithdrawalRequest
{
    [Key]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Amount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(255)]
    public string? BankAccountName { get; set; }

    [StringLength(50)]
    public string? BankAccountNumber { get; set; }

    [StringLength(255)]
    public string? BankName { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Reason { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("WithdrawalRequests")]
    public virtual StudentProfile Student { get; set; } = null!;
}
