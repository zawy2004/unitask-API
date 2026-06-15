using System;

namespace Unitask.Application.DTOs.Contracts;

// ============================================================
// DTOs cho quy trình tranh chấp B1–B4.
// ============================================================

public class DisputeResponse
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public string? MilestoneTitle { get; set; }
    public Guid ContractId { get; set; }
    public Guid RaisedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "NEGOTIATION";
    public string? Decision { get; set; }
    public int? StudentPercent { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? AppealDeadline { get; set; }
}

/// <summary>B1 — mở tranh chấp trên một milestone.</summary>
public class OpenDisputeRequest
{
    public Guid MilestoneId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>B3 — hòa giải viên ra quyết định.</summary>
public class ResolveDisputeRequest
{
    /// <summary>RELEASE | REFUND | SPLIT</summary>
    public string Decision { get; set; } = string.Empty;
    /// <summary>% trả cho người thực hiện khi SPLIT.</summary>
    public int? StudentPercent { get; set; }
    public string? Note { get; set; }
}

/// <summary>B4 — kháng cáo quyết định tạm thời.</summary>
public class AppealDisputeRequest
{
    public string? Reason { get; set; }
}
