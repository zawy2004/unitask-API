using System.Collections.Generic;
using Unitask.Application.Common.Interfaces;

namespace Unitask.Application.Common;

/// <summary>
/// Ánh xạ mỗi <see cref="EmailTemplate"/> tới tên file HTML và tiêu đề mặc định.
/// Tiêu đề cũng hỗ trợ biến <c>{{tên}}</c> (được thay cùng lúc với nội dung).
/// </summary>
public static class EmailTemplateCatalog
{
    public record TemplateInfo(string FileName, string Subject);

    public static readonly IReadOnlyDictionary<EmailTemplate, TemplateInfo> Map =
        new Dictionary<EmailTemplate, TemplateInfo>
        {
            [EmailTemplate.EmailVerification]   = new("01-email-verification.html",   "Xác thực tài khoản UniTask 🎉"),
            [EmailTemplate.ResetPassword]       = new("02-reset-password.html",       "Đặt lại mật khẩu UniTask"),
            [EmailTemplate.SecurityAlert]       = new("03-security-alert.html",       "⚠️ Cảnh báo bảo mật tài khoản của bạn"),
            [EmailTemplate.NewApplication]      = new("04-new-application.html",      "📩 Ứng viên mới cho dự án {{jobTitle}}"),
            [EmailTemplate.ShortlistedInterview]= new("05-shortlisted-interview.html","🎉 Bạn được mời phỏng vấn dự án {{jobTitle}}"),
            [EmailTemplate.OfferReceived]       = new("06-offer-received.html",       "🤝 Bạn nhận được Offer từ {{businessName}}"),
            [EmailTemplate.MilestoneFunded]     = new("07-milestone-funded.html",     "🔒 Mốc \"{{milestoneName}}\" đã được ký quỹ"),
            [EmailTemplate.WorkSubmitted]       = new("08-work-submitted.html",       "📦 Sinh viên đã nộp bài mốc \"{{milestoneName}}\""),
            [EmailTemplate.RevisionRequested]   = new("09-revision-requested.html",   "✏️ Yêu cầu chỉnh sửa mốc \"{{milestoneName}}\""),
            [EmailTemplate.MilestoneApproved]   = new("10-milestone-approved.html",   "🎊 Mốc \"{{milestoneName}}\" đã được giải ngân"),
            [EmailTemplate.UnreadMessage]       = new("11-unread-message.html",       "💬 Bạn có {{unreadCount}} tin nhắn chưa đọc"),
            [EmailTemplate.DeadlineReminder]    = new("12-deadline-reminder.html",    "⏰ Mốc \"{{milestoneName}}\" sắp đến hạn"),
            [EmailTemplate.AutoApproveWarning]  = new("13-auto-approve-warning.html", "⏳ Mốc \"{{milestoneName}}\" sắp tự động giải ngân"),
            [EmailTemplate.WithdrawalApproved]  = new("14-withdrawal-approved.html",  "✅ Rút tiền {{amount}} thành công"),
            [EmailTemplate.ReviewReminder]      = new("15-review-reminder.html",      "⭐ Đánh giá đối tác dự án {{projectName}}"),
            [EmailTemplate.BusinessApproved]    = new("16-business-approved.html",    "🎉 Tài khoản doanh nghiệp {{businessName}} đã được duyệt"),
        };
}
