using System;

namespace Unitask.Application.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string Message { get; set; } = null!;

    public Guid? RelatedJobId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }
}
