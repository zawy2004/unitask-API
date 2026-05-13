using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("CreatedAt", Name = "idx_notification_created_at")]
[Index("IsRead", Name = "idx_notification_is_read")]
[Index("UserId", Name = "idx_notification_user")]
public partial class Notification
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(100)]
    public string? Type { get; set; }

    [StringLength(255)]
    public string? Title { get; set; }

    public string Message { get; set; } = null!;

    public Guid? RelatedJobId { get; set; }

    public Guid? RelatedUserId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    [ForeignKey("RelatedJobId")]
    [InverseProperty("Notifications")]
    public virtual Job? RelatedJob { get; set; }

    [ForeignKey("RelatedUserId")]
    [InverseProperty("NotificationRelatedUsers")]
    public virtual User? RelatedUser { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("NotificationUsers")]
    public virtual User User { get; set; } = null!;
}
