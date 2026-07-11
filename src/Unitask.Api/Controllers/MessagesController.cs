using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.DTOs.Common;
using Unitask.Application.DTOs.Conversations;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class MessagesController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public MessagesController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<PagedResult<MessageListItemResponse>>> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = _dbContext.Messages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .AsQueryable();

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(m => new MessageListItemResponse
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                Content = m.Content,
                AttachmentUrl = m.AttachmentUrl,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt,
                Sender = new UserBriefDto
                {
                    Id = m.Sender.Id,
                    Name = m.Sender.FullName,
                    AvatarUrl = m.Sender.AvatarUrl
                }
            })
            .ToListAsync();

        return Ok(new PagedResult<MessageListItemResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [Authorize]
    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null)
        {
            return NotFound();
        }

        var moderation = MessageModerationService.Analyze(request.Content);
        var msgId = Guid.NewGuid();

        var message = new Unitask.Domain.Entities.Message
        {
            Id = msgId,
            ConversationId = conversationId,
            SenderId = userId.Value,
            Content = request.Content,
            AttachmentUrl = request.AttachmentUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        if (moderation.Flagged)
        {
            try
            {
                // MessageFlags là entity EF chính thức (bảng do schema quản lý) — dùng EF thay raw SQL.
                _dbContext.MessageFlags.Add(new Unitask.Domain.Entities.MessageFlag
                {
                    Id = Guid.NewGuid(),
                    MessageId = msgId,
                    ConversationId = conversationId,
                    SenderId = userId.Value,
                    Content = request.Content,
                    Reasons = string.Join(",", moderation.Reasons),
                    CreatedAt = DateTime.UtcNow,
                });
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // Lưu moderation thất bại nhưng tin nhắn đã gửi OK — không chặn luồng.
            }
        }

        return Ok(new { flagged = moderation.Flagged });
    }
}

