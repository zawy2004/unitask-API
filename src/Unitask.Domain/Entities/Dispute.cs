using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

/// <summary>
/// Tranh chấp về một Milestone (quy trình B1–B4).
/// Vòng đời Status: NEGOTIATION → MEDIATION → RESOLVED → (APPEAL) → CLOSED.
/// Quyết định (Decision) ảnh hưởng ngay tới Escrow: RELEASE / REFUND / SPLIT.
/// </summary>
[Index("MilestoneId", Name = "idx_dispute_milestone")]
[Index("Status", Name = "idx_dispute_status")]
public partial class Dispute
{
    [Key]
    public Guid Id { get; set; }

    public Guid MilestoneId { get; set; }

    public Guid ContractId { get; set; }

    /// <summary>User mở tranh chấp.</summary>
    public Guid RaisedByUserId { get; set; }

    public string Reason { get; set; } = null!;

    /// <summary>NEGOTIATION | MEDIATION | RESOLVED | APPEAL | CLOSED</summary>
    [StringLength(20)]
    public string Status { get; set; } = "NEGOTIATION";

    /// <summary>Hòa giải viên (admin) phụ trách.</summary>
    public Guid? MediatorId { get; set; }

    /// <summary>RELEASE (trả người làm) | REFUND (hoàn DN) | SPLIT (chia theo %).</summary>
    [StringLength(20)]
    public string? Decision { get; set; }

    /// <summary>% trả cho người thực hiện khi Decision = SPLIT.</summary>
    public int? StudentPercent { get; set; }

    public string? DecisionNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    /// <summary>Hạn kháng cáo (7 ngày sau quyết định tạm thời).</summary>
    public DateTime? AppealDeadline { get; set; }

    [ForeignKey("MilestoneId")]
    public virtual Milestone Milestone { get; set; } = null!;
}
