using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
    /// <summary>Sandbox/Demo: doanh nghiệp bỏ qua bước "Chờ Admin duyệt" (KHÔNG bỏ xác thực email của sinh viên).</summary>
    private readonly bool _autoApprove;
    /// <summary>Sandbox/Demo: trả mã OTP về client để test khi chưa cấu hình SMTP. KHÔNG bật ở production.</summary>
    private readonly bool _exposeOtp;

    private const int OtpExpiryMinutes = 10;

    public AuthService(
        UnitaskDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtOptions,
        IEmailService emailService,
        IOptions<EmailSettings> emailOptions,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtOptions.Value;
        _emailService = emailService;
        _emailSettings = emailOptions.Value;
        _autoApprove = configuration.GetValue<bool>("Sandbox:AutoApprove");
        _exposeOtp = configuration.GetValue<bool>("Sandbox:ExposeOtp");
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
            // Mọi tài khoản đều phải xác thực email (OTP) trước. Doanh nghiệp sau đó còn chờ admin duyệt.
            IsActive = false,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Gửi OTP xác thực email cho TẤT CẢ tài khoản mới (sinh viên & doanh nghiệp) — bắt buộc để đảm bảo an ninh.
        var devOtp = await SendOtpAsync(user, cancellationToken);

        var result = CreateAuthResult(user);
        result.NeedsApproval = isBusiness && !_autoApprove;
        result.NeedsEmailVerification = true;
        result.DevOtp = _exposeOtp ? devOtp : null;
        return result;
    }

    /// <summary>Tạo thông báo cho admin khi có doanh nghiệp (đã xác thực email) chờ phê duyệt.</summary>
    private async Task NotifyAdminsBusinessApprovalAsync(User business, CancellationToken ct)
    {
        var admins = await _dbContext.Users.AsNoTracking()
            .Where(u => u.UserType == "admin" && (u.IsActive ?? true))
            .Select(u => u.Id)
            .ToListAsync(ct);

        foreach (var adminId in admins)
        {
            _dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = adminId,
                Type = "business_approval",
                Title = "Yêu cầu phê duyệt tài khoản doanh nghiệp",
                Message = $"{business.FullName} ({business.Email}) đã xác thực email và đang chờ phê duyệt.",
                RelatedUserId = business.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });
        }
    }

    public async Task<AuthResult> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tài khoản với email này.");

        var isBusiness = string.Equals(user.UserType, "business", StringComparison.OrdinalIgnoreCase);

        // Đã xác thực email rồi → không tiêu thêm OTP. DN chưa được duyệt thì vẫn báo "chờ phê duyệt".
        if (user.IsVerified == true)
        {
            var already = CreateAuthResult(user);
            already.NeedsApproval = isBusiness && !_autoApprove && user.IsActive != true;
            return already;
        }

        var normalized = (code ?? string.Empty).Trim();
        var now = DateTime.UtcNow;
        var match = await _dbContext.EmailVerifications
            .Where(v => v.UserId == user.Id && v.Code == normalized
                && v.ConsumedAt == null && v.ExpiresAt > now)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (match is null)
            throw new InvalidOperationException("Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại hoặc gửi lại mã.");

        match.ConsumedAt = DateTime.UtcNow;

        user.IsVerified = true;

        var needsApproval = false;
        if (!isBusiness || _autoApprove)
        {
            // Sinh viên, hoặc doanh nghiệp ở chế độ Sandbox auto-approve → kích hoạt ngay.
            user.IsActive = true;
        }
        else
        {
            // Doanh nghiệp: đã xác thực email → vào hàng chờ admin duyệt, chưa kích hoạt.
            user.IsActive = false;
            needsApproval = true;
            await NotifyAdminsBusinessApprovalAsync(user, cancellationToken);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = CreateAuthResult(user);
        result.NeedsApproval = needsApproval;
        return result;
    }

    public async Task<string?> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        // Không tiết lộ email có tồn tại hay không. Chỉ gửi nếu là tài khoản chưa xác thực (SV & DN).
        if (user is null || user.IsVerified == true) return null;
        var code = await SendOtpAsync(user, cancellationToken);
        return _exposeOtp ? code : null;
    }

    /// <summary>Sinh mã OTP 6 chữ số, lưu vào EmailVerifications và gửi email. Trả về mã vừa sinh.</summary>
    private async Task<string> SendOtpAsync(User user, CancellationToken ct)
    {
        var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var expires = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

        _dbContext.EmailVerifications.Add(new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = code,
            ExpiresAt = expires,
            ConsumedAt = null,
            CreatedAt = DateTime.UtcNow,
        });
        await _dbContext.SaveChangesAsync(ct);

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

        // Demo/Sandbox: ghi mã OTP ra log để tester lấy khi chưa cấu hình SMTP.
        if (_exposeOtp)
        {
            Console.WriteLine($"[Sandbox OTP] {user.Email} -> {code} (hết hạn sau {OtpExpiryMinutes} phút)");
        }

        return code;
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
