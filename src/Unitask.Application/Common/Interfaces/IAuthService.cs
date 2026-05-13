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
}
