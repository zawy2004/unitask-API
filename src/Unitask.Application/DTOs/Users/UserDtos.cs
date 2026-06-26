using System;

namespace Unitask.Application.DTOs.Users;

public class UserProfileResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Bio { get; set; }

    public string UserType { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public bool? IsActive { get; set; }

    public int? ReputationScore { get; set; }

    public DateTime? SuspendedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? AvatarUrl { get; set; }
}

/// <summary>Body cho POST /api/users/{id}/sanction (khung vi phạm M1–M3).</summary>
public class SanctionRequest
{
    /// <summary>M1 | M2 | M3</summary>
    public string Level { get; set; } = string.Empty;
    /// <summary>Số ngày đình chỉ cho M2 (7–30, mặc định 7).</summary>
    public int? Days { get; set; }
    public string? Reason { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }
}

// ====== Quản lý người dùng (Admin) ======

/// <summary>1 dòng trong bảng quản lý người dùng của Admin.</summary>
public class AdminUserListItem
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string UserType { get; set; } = null!;
    public bool? IsVerified { get; set; }
    public bool? IsActive { get; set; }
    public int? ReputationScore { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

/// <summary>Body cho POST /api/users (Admin tạo tài khoản mới).</summary>
public class AdminCreateUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    /// <summary>student | business | admin</summary>
    public string UserType { get; set; } = "student";
}

/// <summary>Body cho PUT /api/users/{id}/admin (Admin sửa thông tin tài khoản).</summary>
public class AdminUpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    /// <summary>student | business | admin</summary>
    public string? UserType { get; set; }
    public bool? IsVerified { get; set; }
}

/// <summary>Body cho PUT /api/users/{id}/status (Admin vô hiệu hóa / kích hoạt).</summary>
public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class VerifyEmailRequest
{
    public string VerificationCode { get; set; } = null!;
}

public class UpdateRoleRequest
{
    public string UserType { get; set; } = null!;
}

public class RejectAccountRequest
{
    public string? Reason { get; set; }
}
