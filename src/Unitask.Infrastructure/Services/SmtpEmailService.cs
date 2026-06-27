using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Unitask.Application.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Settings;

namespace Unitask.Infrastructure.Services;

/// <summary>
/// Hiện thực <see cref="IEmailService"/> dùng MailKit gửi qua SMTP (Gmail + App Password).
///
/// - Đọc file HTML template trong thư mục "EmailTemplates" (copy sang output khi build).
/// - Thay biến dạng <c>{{tên}}</c> trong cả nội dung lẫn tiêu đề.
/// - Nếu <see cref="EmailSettings.Enabled"/> = false thì chỉ log, không gửi (tiện chạy local).
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _templatesDir;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _templatesDir = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");
    }

    public async Task SendTemplateAsync(
        EmailTemplate template,
        string toEmail,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        if (!EmailTemplateCatalog.Map.TryGetValue(template, out var info))
        {
            throw new InvalidOperationException($"Không tìm thấy cấu hình template cho {template}.");
        }

        var path = Path.Combine(_templatesDir, info.FileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Không tìm thấy file template email: {path}");
        }

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);

        // Bổ sung biến mặc định nếu chưa được cung cấp.
        var vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in variables) vars[kv.Key] = kv.Value ?? string.Empty;
        if (!vars.ContainsKey("userEmail")) vars["userEmail"] = toEmail;

        var body = ReplacePlaceholders(html, vars);
        var subject = ReplacePlaceholders(info.Subject, vars);

        await SendRawAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendRawAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "[Email] DISABLED — bỏ qua gửi mail tới {To} (subject: {Subject})", toEmail, subject);
            return;
        }

        var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var secureOption = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect      // cổng 465
                : SecureSocketOptions.StartTls;         // cổng 587

            await client.ConnectAsync(_settings.Host, _settings.Port, secureOption, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("[Email] Đã gửi tới {To} (subject: {Subject})", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Gửi thất bại tới {To} (subject: {Subject})", toEmail, subject);
            throw;
        }
    }

    /// <summary>Thay mọi <c>{{key}}</c> trong <paramref name="input"/> bằng giá trị tương ứng.</summary>
    private static string ReplacePlaceholders(string input, IReadOnlyDictionary<string, string> vars)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new StringBuilder(input);
        foreach (var kv in vars)
        {
            sb.Replace("{{" + kv.Key + "}}", kv.Value);
        }
        return sb.ToString();
    }
}
