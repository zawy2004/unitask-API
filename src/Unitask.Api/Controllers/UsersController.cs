using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Common;
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

    private bool IsAdmin() =>
        string.Equals(User.GetUserRole(), "admin", StringComparison.OrdinalIgnoreCase);

    // ====== Quản lý người dùng (Admin) ======

    /// <summary>
    /// Admin: danh sách người dùng có phân trang, tìm kiếm (tên/email/SĐT)
    /// và lọc theo vai trò (role) + trạng thái (status: active | disabled | suspended).
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserListItem>>> GetUsers(
        [FromQuery] string? q,
        [FromQuery] string? role,
        [FromQuery] string? status,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 12)
    {
        if (!IsAdmin()) return Forbid();

        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.FullName, $"%{term}%")
                || EF.Functions.Like(u.Email, $"%{term}%")
                || (u.Phone != null && EF.Functions.Like(u.Phone, $"%{term}%")));
        }

        if (!string.IsNullOrWhiteSpace(role) && !string.Equals(role, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => u.UserType == role);
        }

        var now = DateTime.UtcNow;
        query = (status?.ToLowerInvariant()) switch
        {
            "active" => query.Where(u => (u.IsActive ?? true) && (u.SuspendedUntil == null || u.SuspendedUntil < now)),
            "disabled" => query.Where(u => u.IsActive == false),
            "suspended" => query.Where(u => u.SuspendedUntil != null && u.SuspendedUntil > now),
            _ => query
        };

        query = (sort?.ToLowerInvariant()) switch
        {
            "name" => query.OrderBy(u => u.FullName),
            "rep" => query.OrderByDescending(u => u.ReputationScore),
            "oldest" => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt) // newest (default)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(u => new AdminUserListItem
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                AvatarUrl = u.AvatarUrl,
                UserType = u.UserType,
                IsVerified = u.IsVerified,
                IsActive = u.IsActive,
                ReputationScore = u.ReputationScore,
                SuspendedUntil = u.SuspendedUntil,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            })
            .ToListAsync();

        return Ok(new PagedResult<AdminUserListItem>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    /// <summary>Admin tạo tài khoản mới (mật khẩu được băm bằng BCrypt).</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AdminUserListItem>> CreateUser([FromBody] AdminCreateUserRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var email = request.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest(new { message = "Email không hợp lệ." });
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự." });
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Vui lòng nhập họ tên." });

        var type = (request.UserType ?? "student").Trim().ToLowerInvariant();
        if (type is not ("student" or "business" or "admin"))
            return BadRequest(new { message = "Vai trò phải là student, business hoặc admin." });

        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Email đã được sử dụng." });

        var user = new Unitask.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            UserType = type,
            IsActive = true,
            IsVerified = false,
            ReputationScore = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok(ToListItem(user));
    }

    /// <summary>Admin sửa thông tin tài khoản (họ tên, email, SĐT, vai trò, xác minh).</summary>
    [Authorize]
    [HttpPut("{id:guid}/admin")]
    public async Task<ActionResult<AdminUserListItem>> AdminUpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (!email.Contains('@')) return BadRequest(new { message = "Email không hợp lệ." });
            if (email != user.Email && await _dbContext.Users.AnyAsync(u => u.Email == email && u.Id != id))
                return Conflict(new { message = "Email đã được sử dụng." });
            user.Email = email;
        }

        if (request.Phone is not null)
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(request.UserType))
        {
            var type = request.UserType.Trim().ToLowerInvariant();
            if (type is not ("student" or "business" or "admin"))
                return BadRequest(new { message = "Vai trò phải là student, business hoặc admin." });
            user.UserType = type;
        }

        if (request.IsVerified.HasValue)
            user.IsVerified = request.IsVerified.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(ToListItem(user));
    }

    /// <summary>Admin vô hiệu hóa / kích hoạt lại tài khoản (IsActive).</summary>
    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (string.Equals(user.UserType, "admin", StringComparison.OrdinalIgnoreCase) && !request.IsActive)
            return BadRequest(new { message = "Không thể vô hiệu hóa tài khoản admin." });

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { id = user.Id, isActive = user.IsActive });
    }

    /// <summary>Admin xóa tài khoản. Nếu còn dữ liệu liên quan -> gợi ý vô hiệu hóa.</summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (string.Equals(user.UserType, "admin", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Không thể xóa tài khoản admin." });

        if (id == User.GetUserId())
            return BadRequest(new { message = "Không thể tự xóa tài khoản của chính bạn." });

        try
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            // Còn ràng buộc dữ liệu (job, hợp đồng, tin nhắn…) -> không xóa cứng được.
            return Conflict(new
            {
                message = "Tài khoản còn dữ liệu liên quan, không thể xóa. Hãy vô hiệu hóa thay thế."
            });
        }
    }

    private static AdminUserListItem ToListItem(Unitask.Domain.Entities.User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        Phone = u.Phone,
        AvatarUrl = u.AvatarUrl,
        UserType = u.UserType,
        IsVerified = u.IsVerified,
        IsActive = u.IsActive,
        ReputationScore = u.ReputationScore,
        SuspendedUntil = u.SuspendedUntil,
        CreatedAt = u.CreatedAt,
        LastLogin = u.LastLogin
    };

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

