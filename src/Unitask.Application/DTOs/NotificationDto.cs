using System;

namespace Unitask.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string Message { get; set; } = null!;
    public Guid? RelatedJobId { get; set; }
    public Guid? RelatedUserId { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

