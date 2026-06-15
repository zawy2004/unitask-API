using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Email", Name = "UQ__Users__A9D10534FF2A9ABE", IsUnique = true)]
[Index("CreatedAt", Name = "idx_users_created_at")]
[Index("Email", Name = "idx_users_email")]
[Index("UserType", Name = "idx_users_user_type")]
public partial class User
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public string? Bio { get; set; }

    [StringLength(50)]
    public string UserType { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>Điểm uy tín (mặc định 100). Vi phạm sẽ bị trừ điểm (khung M1–M3).</summary>
    public int? ReputationScore { get; set; }

    /// <summary>Bị đình chỉ đến thời điểm này (M2). Null = không bị đình chỉ.</summary>
    public DateTime? SuspendedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    [InverseProperty("ReportedBy")]
    public virtual ICollection<AdminReport> AdminReportReportedBies { get; set; } = new List<AdminReport>();

    [InverseProperty("ReportedUser")]
    public virtual ICollection<AdminReport> AdminReportReportedUsers { get; set; } = new List<AdminReport>();

    [InverseProperty("Author")]
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    [InverseProperty("User")]
    public virtual BusinessProfile? BusinessProfile { get; set; }

    [InverseProperty("User1")]
    public virtual ICollection<Conversation> ConversationUser1s { get; set; } = new List<Conversation>();

    [InverseProperty("User2")]
    public virtual ICollection<Conversation> ConversationUser2s { get; set; } = new List<Conversation>();

    [InverseProperty("Sender")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [InverseProperty("RelatedUser")]
    public virtual ICollection<Notification> NotificationRelatedUsers { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    [InverseProperty("FromUser")]
    public virtual ICollection<Review> ReviewFromUsers { get; set; } = new List<Review>();

    [InverseProperty("ToUser")]
    public virtual ICollection<Review> ReviewToUsers { get; set; } = new List<Review>();

    [InverseProperty("User")]
    public virtual StudentProfile? StudentProfile { get; set; }
}
