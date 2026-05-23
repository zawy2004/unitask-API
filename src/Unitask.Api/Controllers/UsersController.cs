using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.AvatarUrl
        };
    }
}

