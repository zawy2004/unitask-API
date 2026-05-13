using System;

namespace Unitask.Application.DTOs;

public class WithdrawalRequestDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Reason { get; set; }
}

