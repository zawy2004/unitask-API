using System;
using System.Collections.Generic;
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
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;

    private const int OtpExpiryMinutes = 10;

    public AuthService(
        UnitaskDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtOptions,
        IEmailService emailService,
        IOptions<EmailSettings> emailOptions)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtOptions.Value;
        _emailService = emailService;
        _emailSettings = emailOptions.Value;
    }

    /// <summary>Dòng OTP đọc từ bảng EmailVerifications (raw SQL).</summary>
    private sealed class OtpRow
    {
        public Guid Id { get; set; }
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
            if (user.IsVerified != true)
                throw new UnauthorizedAccessException("Tài khoản chưa được xác thực email. Vui lòng nhập mã OTP đã gửi tới email của bạn để kích hoạt.");
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
            // Doanh nghiệp: chờ admin duyệt. Sinh viên: chờ xác thực email (OTP) → cũng để IsActive=false.
            IsActive = false,
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

        // Sinh viên: gửi OTP xác thực email để kích hoạt tài khoản.
        if (!isBusiness)
        {
            await SendOtpAsync(user, cancellationToken);
        }

        var result = CreateAuthResult(user);
        result.NeedsApproval = isBusiness;
        result.NeedsEmailVerification = !isBusiness;
        return result;
    }

    public async Task<AuthResult> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tài khoản với email này.");

        if (user.IsVerified == true && user.IsActive == true)
            return CreateAuthResult(user); // đã xác thực rồi

        var normalized = (code ?? string.Empty).Trim();
        var match = await _dbContext.Database
            .SqlQuery<OtpRow>($@"SELECT TOP 1 Id FROM EmailVerifications
                WHERE UserId = {user.Id} AND Code = {normalized}
                  AND ConsumedAt IS NULL AND ExpiresAt > {DateTime.UtcNow}
                ORDER BY CreatedAt DESC")
            .ToListAsync(cancellationToken);

        if (match.Count == 0)
            throw new InvalidOperationException("Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại hoặc gửi lại mã.");

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE EmailVerifications SET ConsumedAt = {DateTime.UtcNow} WHERE Id = {match[0].Id}", cancellationToken);

        user.IsVerified = true;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResult(user);
    }

    public async Task ResendOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        // Không tiết lộ email có tồn tại hay không. Chỉ gửi nếu là tài khoản chưa xác thực.
        if (user is null || user.IsVerified == true) return;
        if (string.Equals(user.UserType, "business", StringComparison.OrdinalIgnoreCase)) return;
        await SendOtpAsync(user, cancellationToken);
    }

    /// <summary>Sinh mã OTP 6 chữ số, lưu vào EmailVerifications và gửi email.</summary>
    private async Task SendOtpAsync(User user, CancellationToken ct)
    {
        var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var expires = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"INSERT INTO EmailVerifications (Id, UserId, Code, ExpiresAt, ConsumedAt, CreatedAt)
               VALUES ({Guid.NewGuid()}, {user.Id}, {code}, {expires}, NULL, {DateTime.UtcNow})", ct);

        try
        {
            await _emailService.SendTemplateAsync(EmailTemplate.EmailVerification, user.Email,
                new Dictionary<string, string>
                {
                    ["userName"] = user.FullName,
                    ["otpCode"] = code,
                    ["verifyUrl"] = $"{_emailSettings.FrontendBaseUrl}/verify-email?email={Uri.EscapeDataString(user.Email)}",
                }, ct);
        }
        catch
        {
            // Không chặn đăng ký nếu gửi mail lỗi; user có thể bấm "gửi lại".
        }
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
