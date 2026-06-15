using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

/// <summary>
/// Sản phẩm Sinh viên nộp cho một Milestone. Mỗi lần nộp là 1 bản ghi mới (giữ lịch sử).
/// ClientFeedback được ghi khi Business bấm "Request Changes".
/// </summary>
[Index("MilestoneId", Name = "idx_submission_milestone")]
[Index("StudentId", Name = "idx_submission_student")]
public partial class Submission
{
    [Key]
    public Guid Id { get; set; }

    public Guid MilestoneId { get; set; }

    /// <summary>Trỏ tới StudentProfiles.Id.</summary>
    public Guid StudentId { get; set; }

    public string? FileUrl { get; set; }

    public string? CoverLetter { get; set; }

    /// <summary>Lý do Business yêu cầu chỉnh sửa (nếu có).</summary>
    public string? ClientFeedback { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("MilestoneId")]
    [InverseProperty("Submissions")]
    public virtual Milestone Milestone { get; set; } = null!;

    [ForeignKey("StudentId")]
    public virtual StudentProfile Student { get; set; } = null!;
}
