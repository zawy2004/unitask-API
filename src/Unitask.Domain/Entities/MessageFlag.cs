using System;
using System.ComponentModel.DataAnnotations;

namespace Unitask.Domain.Entities;

public class MessageFlag
{
    [Key]
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = null!;

    public string Reasons { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
