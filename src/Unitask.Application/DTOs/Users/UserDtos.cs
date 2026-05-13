using System;

namespace Unitask.Application.DTOs.Users;

public class UserProfileResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Bio { get; set; }

    public string UserType { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AvatarUrl { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }
}

public class VerifyEmailRequest
{
    public string VerificationCode { get; set; } = null!;
}
