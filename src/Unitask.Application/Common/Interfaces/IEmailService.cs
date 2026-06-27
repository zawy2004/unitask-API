using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unitask.Application.Common.Interfaces;

/// <summary>
/// Danh mục 15 mẫu email giao dịch của UniTask. Mỗi giá trị ánh xạ tới một file
/// HTML template (xem <c>EmailTemplateCatalog</c>) và một tiêu đề mặc định.
/// </summary>
public enum EmailTemplate
{
    // 1. Tài khoản & Bảo mật
    EmailVerification,
    ResetPassword,
    SecurityAlert,

    // 2. Tuyển dụng & Ứng tuyển
    NewApplication,
    ShortlistedInterview,
    OfferReceived,

    // 3. Làm việc, Ký quỹ & Nghiệm thu
    MilestoneFunded,
    WorkSubmitted,
    RevisionRequested,
    MilestoneApproved,

    // 4. Giao tiếp & Nhắc nhở hệ thống
    UnreadMessage,
    DeadlineReminder,
    AutoApproveWarning,

    // 5. Tài chính & Đánh giá
    WithdrawalApproved,
    ReviewReminder,

    // 6. Quản trị tài khoản
    BusinessApproved
}

/// <summary>
/// Dịch vụ gửi email: render template HTML (thay biến <c>{{tên}}</c>) rồi gửi qua SMTP.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email theo một <paramref name="template"/> tới <paramref name="toEmail"/>,
    /// thay các biến trong template/tiêu đề bằng <paramref name="variables"/>.
    /// Tự động bổ sung biến <c>userEmail</c> nếu chưa có.
    /// </summary>
    Task SendTemplateAsync(
        EmailTemplate template,
        string toEmail,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    /// <summary>Gửi một email HTML tùy ý (không qua template).</summary>
    Task SendRawAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
