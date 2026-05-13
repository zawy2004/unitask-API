using System;

namespace Unitask.Application.DTOs.Conversations;

public class UserBriefDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? AvatarUrl { get; set; }
}

public class ConversationListItemResponse
{
    public Guid Id { get; set; }

    public UserBriefDto OtherUser { get; set; } = new();

    public string? LastMessage { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}

public class MessageListItemResponse
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public UserBriefDto Sender { get; set; } = new();

    public string Content { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = null!;

    public string? AttachmentUrl { get; set; }
}

public class StartConversationRequest
{
    public Guid RecipientId { get; set; }
}
