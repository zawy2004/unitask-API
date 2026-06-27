namespace Unitask.Application.Common.Settings;

/// <summary>
/// Cấu hình gửi email qua SMTP (mặc định dùng Gmail + App Password).
///
/// Gmail SMTP:
///   Host = smtp.gmail.com, Port = 587 (STARTTLS) hoặc 465 (SSL).
///   Username = địa chỉ Gmail của bạn.
///   Password = "Mật khẩu ứng dụng" 16 ký tự (App Password), KHÔNG phải mật khẩu đăng nhập Gmail.
///
/// Bind từ section "EmailSettings" trong appsettings / biến môi trường
/// (ví dụ: EmailSettings__Password trên server).
/// </summary>
public class EmailSettings
{
    /// <summary>Máy chủ SMTP. Gmail: smtp.gmail.com</summary>
    public string Host { get; set; } = "smtp.gmail.com";

    /// <summary>Cổng SMTP. 587 = STARTTLS (khuyến nghị), 465 = SSL.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Dùng SSL ngầm (true cho cổng 465). Với 587 để false (sẽ dùng STARTTLS).</summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>Tài khoản Gmail dùng để đăng nhập SMTP.</summary>
    public string Username { get; set; } = null!;

    /// <summary>Mật khẩu ứng dụng (App Password) 16 ký tự của Gmail.</summary>
    public string Password { get; set; } = null!;

    /// <summary>Email người gửi hiển thị (mặc định = Username nếu để trống).</summary>
    public string FromEmail { get; set; } = null!;

    /// <summary>Tên người gửi hiển thị trong hộp thư người nhận.</summary>
    public string FromName { get; set; } = "UniTask";

    /// <summary>
    /// URL gốc của frontend, dùng để dựng các link trong email (verify, reset, ...).
    /// Ví dụ: https://unitask.io.vn
    /// </summary>
    public string FrontendBaseUrl { get; set; } = "https://unitask.io.vn";

    /// <summary>Bật/tắt gửi email (đặt false khi chạy local nếu chưa cấu hình SMTP).</summary>
    public bool Enabled { get; set; } = true;
}
