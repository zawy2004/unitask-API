using System;

namespace Unitask.Application.DTOs.Payments;

public class PaymentCreateRequest
{
    public Guid JobApplicationId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }
}

public class PaymentListItemResponse
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string Status { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }
}
