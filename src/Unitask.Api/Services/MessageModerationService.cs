using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unitask.Api.Services;

public class ModerationResult
{
    public bool Flagged { get; set; }
    public List<string> Reasons { get; set; } = new();
    public string? SanitizedContent { get; set; }
}

public static class MessageModerationService
{
    private static readonly Regex PhoneRegex = new(
        @"(\+?84|0)(3[2-9]|5[2689]|7[06-9]|8[1-9]|9[0-9])\d{7}",
        RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled);

    private static readonly Regex UrlRegex = new(
        @"https?://[^\s]+|www\.[^\s]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SocialRegex = new(
        @"(facebook\.com|fb\.com|m\.me|zalo\.me|t\.me|telegram\.me|instagram\.com|tiktok\.com|wa\.me|whatsapp)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string[] ContactKeywords = new[]
    {
        "zalo", "telegram", "viber", "whatsapp", "facebook", "messenger",
        "sdt", "số điện thoại", "so dien thoai", "lien he", "liên hệ ngoài",
        "inbox", "add friend", "kết bạn", "ket ban", "gọi cho", "goi cho",
        "gmail", "yahoo", "hotmail", "outlook",
        "skype", "discord", "line app",
    };

    public static ModerationResult Analyze(string content)
    {
        var result = new ModerationResult();
        if (string.IsNullOrWhiteSpace(content))
            return result;

        var lower = content.ToLowerInvariant();

        if (PhoneRegex.IsMatch(content))
            result.Reasons.Add("phone_number");

        if (EmailRegex.IsMatch(content))
            result.Reasons.Add("email_address");

        if (SocialRegex.IsMatch(content))
            result.Reasons.Add("social_media_link");
        else if (UrlRegex.IsMatch(content))
            result.Reasons.Add("external_url");

        foreach (var kw in ContactKeywords)
        {
            if (lower.Contains(kw))
            {
                result.Reasons.Add($"keyword:{kw}");
                break;
            }
        }

        result.Flagged = result.Reasons.Count > 0;
        return result;
    }
}
