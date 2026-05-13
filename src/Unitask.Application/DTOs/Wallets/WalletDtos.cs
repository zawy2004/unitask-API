using System;

namespace Unitask.Application.DTOs.Wallets;

public class WalletResponse
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public decimal? Balance { get; set; }

    public decimal? TotalEarned { get; set; }

    public decimal? TotalWithdrawn { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class WithdrawRequest
{
    public decimal Amount { get; set; }

    public string BankAccountName { get; set; } = null!;

    public string BankAccountNumber { get; set; } = null!;

    public string BankName { get; set; } = null!;
}

public class WithdrawalResponse
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
