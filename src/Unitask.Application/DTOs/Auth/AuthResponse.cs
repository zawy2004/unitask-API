using System;

namespace Unitask.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = null!;

    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;
}
