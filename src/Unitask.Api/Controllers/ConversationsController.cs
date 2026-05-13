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

        var conversations = await _dbContext.Conversations.AsNoTracking()
            .Where(c => c.User1Id == userId.Value || c.User2Id == userId.Value)
            .Include(c => c.User1)
            .Include(c => c.User2)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var result = new List<ConversationListItemResponse>();
        foreach (var conversation in conversations)
        {
            var otherUser = conversation.User1Id == userId.Value ? conversation.User2 : conversation.User1;

            var lastMessage = await _dbContext.Messages.AsNoTracking()
                .Where(m => m.ConversationId == conversation.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            var unreadCount = await _dbContext.Messages.AsNoTracking()
                .CountAsync(m => m.ConversationId == conversation.Id
                    && m.SenderId != userId.Value
                    && (m.IsRead == false || m.IsRead == null));

            result.Add(new ConversationListItemResponse
            {
                Id = conversation.Id,
                OtherUser = new UserBriefDto
                {
                    Id = otherUser.Id,
                    Name = otherUser.FullName,
                    AvatarUrl = otherUser.AvatarUrl
                },
                LastMessage = lastMessage,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount
            });
        }

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

