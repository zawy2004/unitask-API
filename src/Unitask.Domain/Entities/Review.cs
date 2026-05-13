using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Rating", Name = "idx_review_rating")]
[Index("ToUserId", Name = "idx_review_to_user")]
public partial class Review
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid JobApplicationId { get; set; }

    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? SkillEndorsementsJson { get; set; }

    public bool? IsAnonymous { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("ReviewFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [ForeignKey("JobId")]
    [InverseProperty("Reviews")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("JobApplicationId")]
    [InverseProperty("Reviews")]
    public virtual JobApplication JobApplication { get; set; } = null!;

    [ForeignKey("ToUserId")]
    [InverseProperty("ReviewToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
