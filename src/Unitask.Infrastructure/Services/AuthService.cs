using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Models;
using Unitask.Application.Common.Settings;
using Unitask.Application.DTOs.Auth;
using Unitask.Domain.Entities;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UnitaskDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public AuthService(UnitaskDbContext dbContext, IJwtTokenGenerator jwtTokenGenerator, IOptions<JwtSettings> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        if (user.IsActive == false)
        {
            if (string.Equals(user.UserType, "business", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Tài khoản doanh nghiệp của bạn đang chờ admin phê duyệt. Vui lòng chờ thông báo.");
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
        }

        return CreateAuthResult(user);
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var isBusiness = string.Equals(request.UserType, "business", StringComparison.OrdinalIgnoreCase);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            UserType = request.UserType,
            Phone = request.Phone,
            AvatarUrl = request.AvatarUrl,
            Bio = request.Bio,
            IsActive = !isBusiness,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        if (isBusiness)
        {
            var admins = await _dbContext.Users.AsNoTracking()
                .Where(u => u.UserType == "admin" && (u.IsActive ?? true))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            foreach (var adminId in admins)
            {
                _dbContext.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = adminId,
                    Type = "business_approval",
                    Title = "Yêu cầu phê duyệt tài khoản doanh nghiệp",
                    Message = $"{request.FullName} ({request.Email}) đã đăng ký tài khoản doanh nghiệp và đang chờ phê duyệt.",
                    RelatedUserId = user.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = CreateAuthResult(user);
        result.NeedsApproval = isBusiness;
        return result;
    }

    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var principal = ValidateRefreshToken(refreshToken);
        if (principal is null)
        {
            return null;
        }

        var userIdValue = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return CreateAuthResult(user);
    }

    public Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private AuthResult CreateAuthResult(User user)
    {
        return new AuthResult
        {
            User = user,
            Token = _jwtTokenGenerator.GenerateAccessToken(user),
            RefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user)
        };
    }

    private ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return null;
            }

            var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "typ")?.Value;
            if (!string.Equals(tokenType, "refresh", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
