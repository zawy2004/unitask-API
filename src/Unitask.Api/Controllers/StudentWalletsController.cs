using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Wallets;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/wallets")]
public class WalletsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public WalletsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("my-wallet")]
    public async Task<ActionResult<WalletResponse>> GetMyWallet()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
        }

        var wallet = await _dbContext.StudentWallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.StudentId == student.Id);

        if (wallet is null)
        {
            return Ok(new WalletResponse
            {
                Id = Guid.Empty,
                StudentId = student.Id,
                Balance = 0m,
                TotalEarned = 0m,
                TotalWithdrawn = 0m
            });
        }

        return Ok(new WalletResponse
        {
            Id = wallet.Id,
            StudentId = wallet.StudentId,
            Balance = wallet.Balance,
            TotalEarned = wallet.TotalEarned,
            TotalWithdrawn = wallet.TotalWithdrawn,
            UpdatedAt = wallet.UpdatedAt
        });
    }

    [Authorize]
    [HttpPost("withdraw")]
    public async Task<ActionResult<WithdrawalResponse>> Withdraw([FromBody] WithdrawRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
        }

        var withdrawal = new Unitask.Domain.Entities.WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            Amount = request.Amount,
            Status = "pending",
            BankAccountName = request.BankAccountName,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName,
            RequestedAt = DateTime.UtcNow
        };

        _dbContext.WithdrawalRequests.Add(withdrawal);
        await _dbContext.SaveChangesAsync();

        return Ok(new WithdrawalResponse
        {
            Id = withdrawal.Id,
            Amount = withdrawal.Amount,
            Status = withdrawal.Status,
            RequestedAt = withdrawal.RequestedAt,
            CompletedAt = withdrawal.CompletedAt
        });
    }

    [Authorize]
    [HttpGet("withdrawal-history")]
    public async Task<ActionResult<IReadOnlyList<WithdrawalResponse>>> GetWithdrawalHistory()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var student = await _dbContext.StudentProfiles.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId.Value);
        if (student is null)
        {
            return BadRequest(new { message = "Student profile not found." });
        }

        var history = await _dbContext.WithdrawalRequests.AsNoTracking()
            .Where(w => w.StudentId == student.Id)
            .OrderByDescending(w => w.RequestedAt)
            .Select(w => new WithdrawalResponse
            {
                Id = w.Id,
                Amount = w.Amount,
                Status = w.Status,
                RequestedAt = w.RequestedAt,
                CompletedAt = w.CompletedAt
            })
            .ToListAsync();

        return Ok(history);
    }
}

