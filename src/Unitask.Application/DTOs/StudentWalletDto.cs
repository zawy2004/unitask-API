using System;

namespace Unitask.Application.DTOs;

public class StudentWalletDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public decimal? Balance { get; set; }
    public decimal? TotalEarned { get; set; }
    public decimal? TotalWithdrawn { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

