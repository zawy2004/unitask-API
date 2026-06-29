using System;
using System.Threading;
using System.Threading.Tasks;
using Unitask.Application.Common.Models;
using Unitask.Application.DTOs.Auth;

namespace Unitask.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Xác thực email bằng mã OTP. Thành công: kích hoạt tài khoản + trả token đăng nhập.</summary>
    Task<AuthResult> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);

    /// <summary>Gửi lại mã OTP xác thực email cho tài khoản chưa kích hoạt.</summary>
    Task ResendOtpAsync(string email, CancellationToken cancellationToken = default);
}
