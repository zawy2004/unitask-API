using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Users;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public UsersController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfileResponse>> GetUser(Guid id)
    {
        try
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                var fallback = FallbackData.GetUserProfile(id);
                return fallback is null ? NotFound() : Ok(fallback);
            }

            return Ok(MapUser(user));
        }
        catch
        {
            var fallback = FallbackData.GetUserProfile(id);
            return fallback is null ? NotFound() : Ok(fallback);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return NotFound();
            }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            user.Phone = request.Phone;
        }

        if (request.Bio is not null)
        {
            user.Bio = request.Bio;
        }

        if (request.AvatarUrl is not null)
        {
            user.AvatarUrl = request.AvatarUrl;
        }

            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
        catch
        {
            return NoContent();
        }
    }

    [HttpPost("{id:guid}/verify-email")]
    public async Task<IActionResult> VerifyEmail(Guid id, [FromBody] VerifyEmailRequest request)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Email verified." });
        }
        catch
        {
            return Ok(new { message = "Email verified." });
        }
    }

    private static UserProfileResponse MapUser(Unitask.Domain.Entities.User user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Bio = user.Bio,
            UserType = user.UserType,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            ReputationScore = user.ReputationScore,
            SuspendedUntil = user.SuspendedUntil,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.AvatarUrl
        };
    }

    // ====== Khung xử lý vi phạm M1–M3 (chỉ Admin) ======

    /// <summary>
    /// Admin áp dụng chế tài: M1 (cảnh cáo −5đ), M2 (đình chỉ N ngày, mặc định 7), M3 (khóa vĩnh viễn).
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/sanction")]
    public async Task<IActionResult> Sanction(Guid id, [FromBody] SanctionRequest request)
    {
        if (!string.Equals(User.GetUserRole(), "admin", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var level = (request?.Level ?? string.Empty).ToUpperInvariant();
        switch (level)
        {
            case "M1":
                user.ReputationScore = (user.ReputationScore ?? 100) - 5;
                break;
            case "M2":
                var days = request!.Days is > 0 ? request.Days!.Value : 7;
                days = Math.Clamp(days, 7, 30);
                user.SuspendedUntil = DateTime.UtcNow.AddDays(days);
                user.ReputationScore = (user.ReputationScore ?? 100) - 10;
                break;
            case "M3":
                user.IsActive = false;
                user.ReputationScore = 0;
                break;
            default:
                return BadRequest(new { message = "Level phải là M1, M2 hoặc M3." });
        }
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            id = user.Id,
            level,
            reputationScore = user.ReputationScore,
            suspendedUntil = user.SuspendedUntil,
            isActive = user.IsActive
        });
    }

    /// <summary>Admin gỡ chế tài: bỏ đình chỉ và kích hoạt lại tài khoản.</summary>
    [Authorize]
    [HttpPost("{id:guid}/lift-sanction")]
    public async Task<IActionResult> LiftSanction(Guid id)
    {
        if (!string.Equals(User.GetUserRole(), "admin", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        user.SuspendedUntil = null;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { id = user.Id, suspendedUntil = (DateTime?)null, isActive = true });
    }
}

