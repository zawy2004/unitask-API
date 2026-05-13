using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("CreatedAt", Name = "idx_activity_created_at")]
[Index("UserId", Name = "idx_activity_user")]
public partial class ActivityLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    [StringLength(100)]
    public string? ActionType { get; set; }

    [StringLength(100)]
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public string? Description { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ActivityLogs")]
    public virtual User? User { get; set; }
}
