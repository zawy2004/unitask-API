using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("UserId", Name = "UQ__StudentP__1788CC4D8B81EDAD", IsUnique = true)]
[Index("Major", Name = "idx_student_major")]
[Index("University", Name = "idx_student_university")]
[Index("IsVerified", Name = "idx_student_verified")]
public partial class StudentProfile
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(255)]
    public string? StudentEmail { get; set; }

    [StringLength(255)]
    public string? University { get; set; }

    [StringLength(255)]
    public string? Major { get; set; }

    public int? GraduationYear { get; set; }

    public string? CvUrl { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? GradePoint { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? Bio { get; set; }

    public string? PortfolioUrl { get; set; }

    public int? CompletedJobs { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalEarnings { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    [InverseProperty("Student")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();

    [InverseProperty("Student")]
    public virtual StudentWallet? StudentWallet { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("StudentProfile")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}
