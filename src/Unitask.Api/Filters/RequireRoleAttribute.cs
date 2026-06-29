using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Unitask.Api.Extensions;

namespace Unitask.Api.Filters;

/// <summary>
/// Chặn truy cập theo Role đọc từ JWT (claim <c>ClaimTypes.Role</c> = UserType: student/business/admin).
///
/// - Chưa đăng nhập  → 401 + message.
/// - Sai vai trò     → 403 + message phân quyền rõ ràng (thay vì 403 rỗng của [Authorize(Roles=...)]).
///
/// Dùng kèm [Authorize] để pipeline xác thực JWT chạy trước:
/// <code>
/// [Authorize]
/// [RequireRole("student")]
/// public async Task&lt;IActionResult&gt; ApplyToJob(...) { ... }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var principal = context.HttpContext.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Bạn cần đăng nhập để thực hiện thao tác này."
            });
            return;
        }

        var role = principal.GetUserRole();
        var allowed = !string.IsNullOrEmpty(role)
            && _roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

        if (!allowed)
        {
            context.Result = new ObjectResult(new
            {
                message = $"Tài khoản loại '{role ?? "không xác định"}' không có quyền thực hiện thao tác này. " +
                          $"Chỉ {string.Join("/", _roles)} mới được phép."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
