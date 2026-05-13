using System;

namespace Unitask.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid? JobId { get; set; }
    public Guid JobApplicationId { get; set; }
    public Guid? BusinessId { get; set; }
    public Guid? StudentId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string Status { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}

