using System;

namespace Unitask.Application.DTOs.Auth;

public class AuthUserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;
}

public class LoginResponse
{
    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public AuthUserDto User { get; set; } = new();
}

public class RegisterResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public bool NeedsApproval { get; set; }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = null!;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}

public class RefreshTokenResponse
{
    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}
