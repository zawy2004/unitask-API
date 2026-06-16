using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Payments;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;
    private readonly MomoService? _momoService;
    private readonly ILogger<PaymentsController> _logger;
    private static readonly HashSet<string> _processedOrders = new();

    public PaymentsController(UnitaskDbContext dbContext, ILogger<PaymentsController> logger, MomoService? momoService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _momoService = momoService;
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

    [Authorize]
    [HttpPost("momo/create")]
    public async Task<IActionResult> CreateMomoPayment([FromBody] MomoDepositRequest request)
    {
        if (_momoService is null)
            return BadRequest(new { message = "MoMo chưa được cấu hình." });

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var orderId = $"UNITASK_{userId.Value.ToString("N")[..8]}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var orderInfo = $"Nạp tiền UniTask - {request.Amount:N0} VND";

        var result = await _momoService.CreatePaymentAsync(
            orderId,
            (long)request.Amount,
            orderInfo,
            userId.Value.ToString()
        );

        if (result.ResultCode != 0 || string.IsNullOrEmpty(result.PayUrl))
        {
            return BadRequest(new { message = result.Message ?? "Không tạo được giao dịch MoMo." });
        }

        return Ok(new
        {
            payUrl = result.PayUrl,
            orderId,
            requestId = result.RequestId
        });
    }

    [HttpPost("momo/ipn")]
    public async Task<IActionResult> MomoIpn([FromBody] MomoIpnRequest ipn)
    {
        _logger.LogInformation("MoMo IPN received: OrderId={OrderId}, ResultCode={ResultCode}, Amount={Amount}, ExtraData={ExtraData}",
            ipn.OrderId, ipn.ResultCode, ipn.Amount, ipn.ExtraData);

        if (_momoService is null || ipn.OrderId is null)
            return Ok(new { resultCode = 1 });

        if (!_momoService.VerifySignature(ipn))
        {
            _logger.LogWarning("MoMo IPN signature invalid for OrderId={OrderId}", ipn.OrderId);
            return Ok(new { resultCode = 1, message = "Invalid signature" });
        }

        if (ipn.ResultCode != 0)
        {
            _logger.LogInformation("MoMo IPN non-success ResultCode={ResultCode} for OrderId={OrderId}", ipn.ResultCode, ipn.OrderId);
            return Ok(new { resultCode = 0 });
        }

        if (!Guid.TryParse(ipn.ExtraData, out var userId))
        {
            _logger.LogWarning("MoMo IPN invalid ExtraData={ExtraData} for OrderId={OrderId}", ipn.ExtraData, ipn.OrderId);
            return Ok(new { resultCode = 0 });
        }

        await CreditUserBalance(userId, ipn.Amount, ipn.OrderId);

        return Ok(new { resultCode = 0 });
    }

    [Authorize]
    [HttpPost("momo/confirm")]
    public async Task<IActionResult> ConfirmMomoPayment([FromBody] MomoConfirmRequest request)
    {
        if (_momoService is null)
            return BadRequest(new { message = "MoMo chưa được cấu hình." });

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        _logger.LogInformation("MoMo confirm requested: OrderId={OrderId}, UserId={UserId}", request.OrderId, userId);

        var query = await _momoService.QueryPaymentAsync(request.OrderId);

        _logger.LogInformation("MoMo query result: OrderId={OrderId}, ResultCode={ResultCode}, Amount={Amount}",
            request.OrderId, query.ResultCode, query.Amount);

        if (query.ResultCode != 0)
            return Ok(new { success = false, message = query.Message ?? "Giao dịch chưa thành công." });

        await CreditUserBalance(userId.Value, query.Amount, request.OrderId);

        return Ok(new { success = true, amount = query.Amount });
    }

    private async Task CreditUserBalance(Guid userId, long amount, string orderId)
    {
        lock (_processedOrders)
        {
            if (!_processedOrders.Add(orderId))
            {
                _logger.LogInformation("OrderId={OrderId} already processed, skipping", orderId);
                return;
            }
        }

        var student = await _dbContext.StudentProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is not null)
        {
            var wallet = await _dbContext.StudentWallets
                .FirstOrDefaultAsync(w => w.StudentId == student.Id);

            if (wallet is null)
            {
                wallet = new Unitask.Domain.Entities.StudentWallet
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    Balance = amount,
                    TotalEarned = amount,
                    TotalWithdrawn = 0m,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.StudentWallets.Add(wallet);
            }
            else
            {
                wallet.Balance += amount;
                wallet.TotalEarned += amount;
                wallet.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Student wallet updated: UserId={UserId}, Amount={Amount}", userId, amount);
        }

        var business = await _dbContext.BusinessProfiles
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (business is not null)
        {
            business.Balance = (business.Balance ?? 0m) + amount;
            business.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Business balance updated: UserId={UserId}, Amount={Amount}, NewBalance={Balance}", userId, amount, business.Balance);
        }

        if (student is null && business is null)
        {
            _logger.LogWarning("No student or business profile found for UserId={UserId}", userId);
        }
    }

    [HttpGet("momo/return")]
    public async Task<IActionResult> MomoReturn(
        [FromQuery] string? orderId,
        [FromQuery] int resultCode,
        [FromQuery] long amount,
        [FromQuery] string? message)
    {
        _logger.LogInformation("MoMo return: OrderId={OrderId}, ResultCode={ResultCode}, Amount={Amount}", orderId, resultCode, amount);

        if (resultCode == 0 && !string.IsNullOrEmpty(orderId) && _momoService is not null)
        {
            try
            {
                var query = await _momoService.QueryPaymentAsync(orderId);
                _logger.LogInformation("MoMo return query: OrderId={OrderId}, QueryResultCode={ResultCode}, Amount={Amount}, ExtraData={ExtraData}",
                    orderId, query.ResultCode, query.Amount, query.ExtraData);

                if (query.ResultCode == 0 && Guid.TryParse(query.ExtraData, out var userId))
                {
                    await CreditUserBalance(userId, query.Amount, orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MoMo return processing failed for OrderId={OrderId}", orderId);
            }
        }

        var redirectUrl = $"https://www.unitask.io.vn/wallet?momo=return&resultCode={resultCode}&amount={amount}&orderId={orderId}";
        return Redirect(redirectUrl);
    }
}

public class MomoDepositRequest
{
    public decimal Amount { get; set; }
}

public class MomoConfirmRequest
{
    public string OrderId { get; set; } = null!;
}

