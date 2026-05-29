using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Notifications;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public NotificationsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] string? type,
        [FromQuery] int page = 1)
    {
        const int limit = 20;
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = _dbContext.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId.Value)
            .AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(n => n.Type == type);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                RelatedJobId = n.RelatedJobId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResult<NotificationResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateRequest request)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.RecipientId);
        if (!userExists)
        {
            return BadRequest(new { message = "Recipient not found." });
        }

        var notification = new Unitask.Domain.Entities.Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.RecipientId,
            Type = request.Type ?? "system",
            Title = request.Title,
            Message = request.Message,
            RelatedJobId = request.RelatedJobId,
            RelatedUserId = User.GetUserId(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        return Ok(new NotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            RelatedJobId = notification.RelatedJobId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        });
    }

    [Authorize]
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        if (notification is null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        await _dbContext.Notifications
            .Where(n => n.UserId == userId.Value && (n.IsRead == false || n.IsRead == null))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now));

        return NoContent();
    }
}

