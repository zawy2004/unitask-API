using Unitask.Domain.Entities;

namespace Unitask.Application.Common.Models;

public class AuthResult
{
    public User User { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public bool NeedsApproval { get; set; }

    /// <summary>True nếu sinh viên mới cần nhập OTP xác thực email trước khi kích hoạt.</summary>
    public bool NeedsEmailVerification { get; set; }
}
