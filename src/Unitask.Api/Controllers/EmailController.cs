using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Unitask.Application.Common.Interfaces;

namespace Unitask.Api.Controllers;

/// <summary>
/// Endpoint kiểm thử module gửi email. Dùng để xác nhận cấu hình SMTP/Gmail hoạt động.
/// Nên giới hạn quyền truy cập (hoặc xoá) trước khi lên production.
/// </summary>
[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public record SendTestRequest(
        string ToEmail,
        string Template = "EmailVerification",
        Dictionary<string, string>? Variables = null);

    /// <summary>
    /// Gửi thử một email theo template. Ví dụ body:
    /// <code>
    /// {
    ///   "toEmail": "ban@gmail.com",
    ///   "template": "EmailVerification",
    ///   "variables": { "userName": "Gia Huy", "otpCode": "123456", "verifyUrl": "https://..." }
    /// }
    /// </code>
    /// Các giá trị template hợp lệ: xem enum <see cref="EmailTemplate"/>
    /// (EmailVerification, ResetPassword, SecurityAlert, NewApplication, ...).
    /// </summary>
    [HttpPost("send-test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return BadRequest(new { message = "toEmail là bắt buộc." });
        }

        if (!Enum.TryParse<EmailTemplate>(request.Template, ignoreCase: true, out var template))
        {
            return BadRequest(new
            {
                message = $"Template '{request.Template}' không hợp lệ.",
                validTemplates = Enum.GetNames<EmailTemplate>()
            });
        }

        // Bộ biến mẫu để xem trước nhanh nếu client không truyền gì.
        var vars = request.Variables ?? new Dictionary<string, string>
        {
            ["userName"] = "Gia Huy",
            ["otpCode"] = "123456",
            ["verifyUrl"] = "https://unitask.io.vn/verify?token=demo",
        };

        try
        {
            await _emailService.SendTemplateAsync(template, request.ToEmail, vars, ct);
            return Ok(new { message = $"Đã gửi email '{template}' tới {request.ToEmail}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Gửi email thất bại.", detail = ex.Message });
        }
    }
}
