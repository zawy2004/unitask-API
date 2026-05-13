using System;

namespace Unitask.Application.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }
}
