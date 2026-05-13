using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("Category", Name = "idx_faq_category")]
[Index("OrderIndex", Name = "idx_faq_order")]
public partial class FAQ
{
    [Key]
    public Guid Id { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    [StringLength(100)]
    public string? Category { get; set; }

    public int? ViewCount { get; set; }

    public int? HelpfulCount { get; set; }

    public int? OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
