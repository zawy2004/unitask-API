using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Domain.Entities;

[Index("User1Id", "User2Id", Name = "idx_conversation_users")]
public partial class Conversation
{
    [Key]
    public Guid Id { get; set; }

    public Guid User1Id { get; set; }

    public Guid User2Id { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Conversation")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [ForeignKey("User1Id")]
    [InverseProperty("ConversationUser1s")]
    public virtual User User1 { get; set; } = null!;

    [ForeignKey("User2Id")]
    [InverseProperty("ConversationUser2s")]
    public virtual User User2 { get; set; } = null!;
}
