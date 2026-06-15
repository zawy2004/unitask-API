using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("UserId", Name = "UQ__Business__1788CC4DAC86ABAD", IsUnique = true)]
[Index("Industry", Name = "idx_business_industry")]
[Index("IsVerified", Name = "idx_business_verified")]
public partial class BusinessProfile
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(255)]
    public string CompanyName { get; set; } = null!;

    [StringLength(255)]
    public string? CompanyEmail { get; set; }

    [StringLength(255)]
    public string? CompanyWebsite { get; set; }

    [StringLength(50)]
    public string? CompanySize { get; set; }

    [StringLength(255)]
    public string? Industry { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    /// <summary>Mã số thuế (xác thực doanh nghiệp).</summary>
    [StringLength(20)]
    public string? TaxCode { get; set; }

    /// <summary>Ảnh/scan giấy phép kinh doanh.</summary>
    public string? BusinessLicenseUrl { get; set; }

    public string? Address { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public int? CompletedProjects { get; set; }

    /// <summary>Số dư khả dụng của doanh nghiệp (nạp vào để ký quỹ milestone).</summary>
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? Balance { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalSpent { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? Rating { get; set; }

    /// <summary>Số lần từ chối nghiệm thu bị đánh giá vô lý (chính sách 1.4).</summary>
    public int? RejectionStrikes { get; set; }

    /// <summary>Bị khóa tính năng đăng task (sau 3 lần từ chối vô lý).</summary>
    public bool? IsPostingLocked { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Business")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    [InverseProperty("Business")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("BusinessProfile")]
    public virtual User User { get; set; } = null!;
}
