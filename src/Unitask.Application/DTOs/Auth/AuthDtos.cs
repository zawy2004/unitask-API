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

    /// <summary>True nếu doanh nghiệp đã xác thực email nhưng vẫn đang chờ admin phê duyệt.</summary>
    public bool NeedsApproval { get; set; }
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

    public bool NeedsEmailVerification { get; set; }

    /// <summary>Mã OTP demo — chỉ trả về khi bật Sandbox:ExposeOtp (chưa cấu hình SMTP).</summary>
    public string? DevOtp { get; set; }
}

public class VerifyEmailRequest
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class ResendOtpRequest
{
    public string Email { get; set; } = null!;
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
