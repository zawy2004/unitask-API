using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Common;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminMessagesController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public AdminMessagesController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<PagedResult<AdminConversationDto>>> GetAllConversations(
        [FromQuery] string? search,
        [FromQuery] bool? hasFlagged,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = _dbContext.Conversations.AsNoTracking()
            .Include(c => c.User1)
            .Include(c => c.User2)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.User1.FullName.ToLower().Contains(term)
                || c.User2.FullName.ToLower().Contains(term)
                || c.User1.Email.ToLower().Contains(term)
                || c.User2.Email.ToLower().Contains(term));
        }

        if (hasFlagged == true)
        {
            var flaggedConvIds = _dbContext.MessageFlags
                .Select(f => f.ConversationId)
                .Distinct();
            query = query.Where(c => flaggedConvIds.Contains(c.Id));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(c => new AdminConversationDto
            {
                Id = c.Id,
                User1 = new AdminUserBriefDto
                {
                    Id = c.User1.Id,
                    Name = c.User1.FullName,
                    Email = c.User1.Email,
                    UserType = c.User1.UserType
                },
                User2 = new AdminUserBriefDto
                {
                    Id = c.User2.Id,
                    Name = c.User2.FullName,
                    Email = c.User2.Email,
                    UserType = c.User2.UserType
                },
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                LastMessageAt = c.LastMessageAt,
                TotalMessages = c.Messages.Count(),
                FlaggedCount = _dbContext.MessageFlags.Count(f => f.ConversationId == c.Id)
            })
            .ToListAsync();

        return Ok(new PagedResult<AdminConversationDto>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<PagedResult<AdminMessageDto>>> GetConversationMessages(
        Guid conversationId,
        [FromQuery] bool? flaggedOnly,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 200);

        var query = _dbContext.Messages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .AsQueryable();

        if (flaggedOnly == true)
        {
            var flaggedMsgIds = _dbContext.MessageFlags.Select(f => f.MessageId);
            query = query.Where(m => flaggedMsgIds.Contains(m.Id));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(m => new AdminMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderName = m.Sender.FullName,
                SenderEmail = m.Sender.Email,
                SenderType = m.Sender.UserType,
                CreatedAt = m.CreatedAt,
                IsFlagged = _dbContext.MessageFlags.Any(f => f.MessageId == m.Id),
                FlagReasons = _dbContext.MessageFlags.Where(f => f.MessageId == m.Id).Select(f => f.Reasons).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new PagedResult<AdminMessageDto>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [HttpGet("flagged-messages")]
    public async Task<ActionResult<PagedResult<AdminFlaggedMessageDto>>> GetFlaggedMessages(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = _dbContext.MessageFlags.AsNoTracking().AsQueryable();

        var total = await query.CountAsync();
        var flags = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var senderIds = flags.Select(f => f.SenderId).Distinct().ToList();
        var convIds = flags.Select(f => f.ConversationId).Distinct().ToList();

        var senders = await _dbContext.Users.AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var convs = await _dbContext.Conversations.AsNoTracking()
            .Where(c => convIds.Contains(c.Id))
            .Include(c => c.User1).Include(c => c.User2)
            .ToDictionaryAsync(c => c.Id);

        var items = flags.Select(f =>
        {
            senders.TryGetValue(f.SenderId, out var sender);
            convs.TryGetValue(f.ConversationId, out var conv);
            return new AdminFlaggedMessageDto
            {
                Id = f.MessageId,
                Content = f.Content,
                FlagReasons = f.Reasons,
                CreatedAt = f.CreatedAt,
                Sender = sender is not null ? new AdminUserBriefDto
                {
                    Id = sender.Id,
                    Name = sender.FullName,
                    Email = sender.Email,
                    UserType = sender.UserType
                } : new AdminUserBriefDto { Name = "Unknown", Email = "", UserType = "" },
                ConversationId = f.ConversationId,
                ConversationWith = conv is not null
                    ? (conv.User1Id == f.SenderId ? conv.User2.FullName : conv.User1.FullName)
                    : null
            };
        }).ToList();

        return Ok(new PagedResult<AdminFlaggedMessageDto>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = items
        });
    }

    [HttpGet("moderation-stats")]
    public async Task<IActionResult> GetModerationStats()
    {
        var totalMessages = await _dbContext.Messages.CountAsync();
        var flaggedMessages = await _dbContext.MessageFlags.CountAsync();
        var totalConversations = await _dbContext.Conversations.CountAsync();
        var conversationsWithFlags = await _dbContext.MessageFlags
            .Select(f => f.ConversationId)
            .Distinct()
            .CountAsync();

        return Ok(new
        {
            totalMessages,
            flaggedMessages,
            totalConversations,
            conversationsWithFlags,
            flagRate = totalMessages > 0
                ? Math.Round((double)flaggedMessages / totalMessages * 100, 1)
                : 0
        });
    }
}

public class AdminConversationDto
{
    public Guid Id { get; set; }
    public AdminUserBriefDto User1 { get; set; } = null!;
    public AdminUserBriefDto User2 { get; set; } = null!;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int TotalMessages { get; set; }
    public int FlaggedCount { get; set; }
}

public class AdminUserBriefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserType { get; set; } = null!;
}

public class AdminMessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string SenderType { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public bool? IsFlagged { get; set; }
    public string? FlagReasons { get; set; }
}

public class AdminFlaggedMessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string? FlagReasons { get; set; }
    public DateTime? CreatedAt { get; set; }
    public AdminUserBriefDto Sender { get; set; } = null!;
    public Guid ConversationId { get; set; }
    public string? ConversationWith { get; set; }
}
