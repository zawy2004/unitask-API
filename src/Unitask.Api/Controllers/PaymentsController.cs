using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Payments;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public PaymentsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PaymentListItemResponse>>> GetPayments(
        [FromQuery] string? status,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1)
    {
        const int limit = 10;
        var query = _dbContext.Payments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (userId.HasValue)
        {
            var businessIds = _dbContext.BusinessProfiles
                .Where(b => b.UserId == userId.Value)
                .Select(b => b.Id);

            var studentIds = _dbContext.StudentProfiles
                .Where(s => s.UserId == userId.Value)
                .Select(s => s.Id);

            query = query.Where(p =>
                (p.BusinessId.HasValue && businessIds.Contains(p.BusinessId.Value))
                || (p.StudentId.HasValue && studentIds.Contains(p.StudentId.Value)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(p => new PaymentListItemResponse
            {
                Id = p.Id,
                JobApplicationId = p.JobApplicationId,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                CreatedAt = p.CreatedAt,
                ReleasedAt = p.ReleasedAt
            })
            .ToListAsync();

        return Ok(new PagedResult<PaymentListItemResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PaymentListItemResponse>> CreatePayment([FromBody] PaymentCreateRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var business = await _dbContext.BusinessProfiles
            .FirstOrDefaultAsync(b => b.UserId == userId.Value);
        if (business is null)
        {
            return BadRequest(new { message = "Business profile not found." });
        }

        var application = await _dbContext.JobApplications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == request.JobApplicationId);
        if (application is null)
        {
            return BadRequest(new { message = "Job application not found." });
        }

        var payment = new Unitask.Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            JobId = application.JobId,
            BusinessId = business.Id,
            StudentId = application.StudentId,
            Amount = request.Amount,
            Currency = "VND",
            Status = "pending",
            PaymentMethod = request.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return Ok(new PaymentListItemResponse
        {
            Id = payment.Id,
            JobApplicationId = payment.JobApplicationId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            PaymentMethod = payment.PaymentMethod,
            CreatedAt = payment.CreatedAt,
            ReleasedAt = payment.ReleasedAt
        });
    }

    [Authorize]
    [HttpPost("{id:guid}/release")]
    public async Task<IActionResult> ReleasePayment(Guid id)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment is null)
        {
            return NotFound();
        }

        payment.Status = "released";
        payment.ReleasedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> RefundPayment(Guid id)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id);
        if (payment is null)
        {
            return NotFound();
        }

        payment.Status = "refunded";
        payment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}

