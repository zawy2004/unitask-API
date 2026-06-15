using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Contracts;

// ============================================================
// DTOs cho module Hợp đồng / Milestone / Submission.
// Đây là "hợp đồng dữ liệu" giữa API và client — tách khỏi entity DB.
// ============================================================

/// <summary>Một lần nộp bài của Sinh viên (phục vụ hiển thị Box "bài nộp mới").</summary>
public class SubmissionResponse
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public string? FileUrl { get; set; }
    public string? CoverLetter { get; set; }
    public string? ClientFeedback { get; set; }
    public string? ClientEvidenceUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>Milestone kèm bản nộp mới nhất để frontend render badge + nút hành động.</summary>
public class MilestoneResponse
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>PENDING | ESCROWED | UNDER_REVIEW | REVISION | COMPLETED</summary>
    public string Status { get; set; } = "PENDING";
    public DateTime? DueDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    /// <summary>Bản nộp gần nhất — null nếu sinh viên chưa nộp lần nào.</summary>
    public SubmissionResponse? LatestSubmission { get; set; }
}

/// <summary>Hợp đồng kèm danh sách Milestone.</summary>
public class ContractResponse
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string? JobTitle { get; set; }
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public Guid BusinessId { get; set; }
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime? CreatedAt { get; set; }
    public List<MilestoneResponse> Milestones { get; set; } = new();
}

// ---------------------- REQUEST BODIES ----------------------

/// <summary>Body cho POST /api/milestones/{id}/submit (Student nộp bài).</summary>
public class SubmitMilestoneRequest
{
    public string? FileUrl { get; set; }
    public string? CoverLetter { get; set; }
}

/// <summary>Body cho POST /api/milestones/{id}/request-changes (Business yêu cầu sửa).</summary>
public class RequestChangesRequest
{
    /// <summary>Lý do cần sửa — bắt buộc (chính sách 1.4).</summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>Link bằng chứng kèm theo — bắt buộc (chính sách 1.4).</summary>
    public string EvidenceUrl { get; set; } = string.Empty;
}

/// <summary>(Tùy chọn) Body cho escrow — cho phép gắn mã giao dịch cổng thanh toán giả lập.</summary>
public class EscrowMilestoneRequest
{
    public string? PaymentReference { get; set; }
}

/// <summary>Body cho POST /api/milestones/{id}/cancel (Business hủy task — chính sách 1.3).</summary>
public class CancelMilestoneRequest
{
    /// <summary>% tiến độ đã hoàn thành (0–100) — phần này trả cho người thực hiện, phần còn lại hoàn về DN.</summary>
    public int ProgressPercent { get; set; }

    public string? Reason { get; set; }
}

// ---- (Tùy chọn) tạo hợp đồng + milestone khi Business duyệt ứng tuyển ----

public class CreateMilestoneRequest
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CreateContractRequest
{
    public Guid JobApplicationId { get; set; }
    public decimal? FinalPrice { get; set; }
    public List<CreateMilestoneRequest> Milestones { get; set; } = new();
}
