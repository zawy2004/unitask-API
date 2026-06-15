using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

/// <summary>
/// Hợp đồng chính thức giữa Doanh nghiệp và Sinh viên cho một Job.
/// Được tạo sau khi Business duyệt đơn ứng tuyển; là "gốc" của các Milestone.
/// </summary>
[Index("JobId", Name = "idx_contract_job")]
[Index("StudentId", Name = "idx_contract_student")]
[Index("BusinessId", Name = "idx_contract_business")]
[Index("Status", Name = "idx_contract_status")]
public partial class Contract
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    /// <summary>Trỏ tới StudentProfiles.Id (đồng bộ với JobApplications.StudentId).</summary>
    public Guid StudentId { get; set; }

    /// <summary>Trỏ tới BusinessProfiles.Id (đồng bộ với Jobs.BusinessId).</summary>
    public Guid BusinessId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal FinalPrice { get; set; }

    /// <summary>ACTIVE | COMPLETED | CANCELED</summary>
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("JobId")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("StudentId")]
    public virtual StudentProfile Student { get; set; } = null!;

    [ForeignKey("BusinessId")]
    public virtual BusinessProfile Business { get; set; } = null!;

    [InverseProperty("Contract")]
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
}
