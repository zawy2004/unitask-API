using System;
using System.ComponentModel.DataAnnotations;

namespace Unitask.Domain.Entities;

/// <summary>
/// Mã OTP xác thực email khi đăng ký. Trước đây truy cập bằng raw T-SQL;
/// nay là entity EF chính thức để tương thích PostgreSQL.
/// </summary>
public class EmailVerification
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(10)]
    public string Code { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime? CreatedAt { get; set; }
}
