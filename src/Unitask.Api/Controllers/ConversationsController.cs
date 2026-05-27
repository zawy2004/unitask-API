using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Application.DTOs.Conversations;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly UnitaskDbContext _dbContext;

    public ConversationsController(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationListItemResponse>>> GetConversations()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _dbContext.Conversations.AsNoTracking()
            .Where(c => c.User1Id == userId.Value || c.User2Id == userId.Value)
            .Include(c => c.User1)
            .Include(c => c.User2)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationListItemResponse
            {
                Id = c.Id,
                OtherUser = new UserBriefDto
                {
                    Id = c.User1Id == userId.Value ? c.User2.Id : c.User1.Id,
                    Name = c.User1Id == userId.Value ? c.User2.FullName : c.User1.FullName,
                    AvatarUrl = c.User1Id == userId.Value ? c.User2.AvatarUrl : c.User1.AvatarUrl
                },
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                LastMessageAt = c.LastMessageAt,
                UnreadCount = c.Messages
                    .Count(m => m.SenderId != userId.Value && (m.IsRead == false || m.IsRead == null))
            })
            .ToListAsync();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("start")]
    public async Task<ActionResult<ConversationListItemResponse>> StartConversation([FromBody] StartConversationRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var existing = await _dbContext.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c => (c.User1Id == userId.Value && c.User2Id == request.RecipientId)
                || (c.User1Id == request.RecipientId && c.User2Id == userId.Value));

        if (existing is not null)
        {
            return Ok(new ConversationListItemResponse
            {
                Id = existing.Id,
                OtherUser = new UserBriefDto { Id = request.RecipientId }
            });
        }

        var conversation = new Unitask.Domain.Entities.Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = userId.Value,
            User2Id = request.RecipientId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        return Ok(new ConversationListItemResponse
        {
            Id = conversation.Id,
            OtherUser = new UserBriefDto { Id = request.RecipientId }
        });
    }
}

