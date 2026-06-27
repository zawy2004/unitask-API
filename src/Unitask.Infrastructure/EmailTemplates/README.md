# Module gửi Email (Gmail SMTP + MailKit)

Hệ thống gửi email giao dịch của UniTask, dùng **MailKit** kết nối **Gmail SMTP** với **App Password** (miễn phí, tối đa ~500 email/ngày).

## Kiến trúc

| Thành phần | Vị trí |
|-----------|--------|
| `EmailSettings` (cấu hình SMTP) | `Unitask.Application/Common/Settings/EmailSettings.cs` |
| `IEmailService` + enum `EmailTemplate` | `Unitask.Application/Common/Interfaces/IEmailService.cs` |
| `EmailTemplateCatalog` (map template → file + tiêu đề) | `Unitask.Application/Common/EmailTemplateCatalog.cs` |
| `SmtpEmailService` (hiện thực MailKit) | `Unitask.Infrastructure/Services/SmtpEmailService.cs` |
| 15 template HTML | `Unitask.Infrastructure/EmailTemplates/*.html` |
| Endpoint test | `Unitask.Api/Controllers/EmailController.cs` |

## Bước 1 — Tạo App Password cho Gmail

1. Vào https://myaccount.google.com → **Bảo mật (Security)**.
2. Bật **Xác minh 2 bước (2-Step Verification)**.
3. Vào https://myaccount.google.com/apppasswords → nhập tên (vd: `UniTask API`) → **Tạo**.
4. Copy chuỗi **16 ký tự** Google cấp (đây là mật khẩu bỏ vào cấu hình, KHÔNG phải mật khẩu Gmail).

## Bước 2 — Cấu hình

**Local** — sửa `Unitask.Api/appsettings.Development.json` (file này đã gitignore):

```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": false,
  "Username": "ban@gmail.com",
  "Password": "abcd efgh ijkl mnop",   // App Password 16 ký tự (bỏ khoảng trắng cũng được)
  "FromEmail": "ban@gmail.com",
  "FromName": "UniTask",
  "FrontendBaseUrl": "http://localhost:5173",
  "Enabled": true
}
```

**Production (Docker / EC2)** — đặt trong `.env` (xem `.env.example`):

```
EmailSettings__Username=ban@gmail.com
EmailSettings__Password=app_password_16_ky_tu
EmailSettings__FromEmail=ban@gmail.com
```

> Đặt `Enabled=false` nếu muốn tắt gửi mail khi dev (service sẽ chỉ log, không gửi).

## Bước 3 — Test

Chạy API rồi gọi (Swagger hoặc curl):

```bash
curl -X POST http://localhost:5000/api/email/send-test \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "nguoinhan@gmail.com",
    "template": "EmailVerification",
    "variables": { "userName": "Gia Huy", "otpCode": "123456", "verifyUrl": "http://localhost:5173/verify?token=abc" }
  }'
```

## Dùng trong code (inject `IEmailService`)

```csharp
public class AuthService
{
    private readonly IEmailService _email;
    public AuthService(IEmailService email) => _email = email;

    public async Task SendVerification(string email, string name, string otp)
    {
        await _email.SendTemplateAsync(EmailTemplate.EmailVerification, email, new Dictionary<string, string>
        {
            ["userName"]  = name,
            ["otpCode"]   = otp,
            ["verifyUrl"] = $"{_frontendBaseUrl}/verify?code={otp}"
        });
    }
}
```

## Danh sách template & biến

| Enum `EmailTemplate` | File | Biến chính |
|----------------------|------|-----------|
| `EmailVerification` | 01 | userName, otpCode, verifyUrl |
| `ResetPassword` | 02 | userName, resetUrl |
| `SecurityAlert` | 03 | userName, eventType, eventTime, ipAddress, deviceInfo, location, secureAccountUrl |
| `NewApplication` | 04 | businessName, studentName, studentInitial, studentMajor, studentRating, coverLetterExcerpt, jobTitle, viewApplicationUrl |
| `ShortlistedInterview` | 05 | studentName, businessName, jobTitle, chatUrl |
| `OfferReceived` | 06 | studentName, businessName, projectName, offerAmount, duration, startDate, viewOfferUrl |
| `MilestoneFunded` | 07 | studentName, milestoneName, amount, workspaceUrl |
| `WorkSubmitted` | 08 | businessName, studentName, milestoneName, reviewUrl |
| `RevisionRequested` | 09 | studentName, businessName, milestoneName, clientFeedback, workspaceUrl |
| `MilestoneApproved` | 10 | studentName, milestoneName, grossAmount, feePercent, feeAmount, netAmount, walletUrl |
| `UnreadMessage` | 11 | userName, unreadCount, senderName, senderInitial, messagePreview, chatUrl, notificationSettingsUrl |
| `DeadlineReminder` | 12 | studentName, milestoneName, projectName, timeRemaining, deadline, workspaceUrl |
| `AutoApproveWarning` | 13 | businessName, milestoneName, daysSinceSubmit, amount, daysLeft, reviewUrl |
| `WithdrawalApproved` | 14 | studentName, amount, bankName, maskedAccount, transactionId, processedAt, walletUrl |
| `ReviewReminder` | 15 | userName, projectName, partnerName, reviewUrl |

> Biến `userEmail` tự động được thêm (= địa chỉ người nhận) nếu bạn không truyền.
