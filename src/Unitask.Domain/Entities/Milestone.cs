using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

/// <summary>
/// Một giai đoạn thanh toán của Contract.
/// Vòng đời Status: PENDING → ESCROWED → UNDER_REVIEW → (REVISION → UNDER_REVIEW)* → COMPLETED.
/// </summary>
[Index("ContractId", Name = "idx_milestone_contract")]
[Index("Status", Name = "idx_milestone_status")]
public partial class Milestone
{
    [Key]
    public Guid Id { get; set; }

    public Guid ContractId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Amount { get; set; }

    /// <summary>PENDING | ESCROWED | UNDER_REVIEW | REVISION | COMPLETED | CANCELED</summary>
    [StringLength(20)]
    public string Status { get; set; } = "PENDING";

    public DateTime? DueDate { get; set; }

    /// <summary>Thời điểm ký quỹ (mốc tính 48h cho chính sách hủy 1.3).</summary>
    public DateTime? EscrowedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("Milestones")]
    public virtual Contract Contract { get; set; } = null!;

    [InverseProperty("Milestone")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
