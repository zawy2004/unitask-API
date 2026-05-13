using System;

namespace Unitask.Application.DTOs.Businesses;

public class StudentDashboardNotificationDto
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string Message { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }
}
