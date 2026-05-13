namespace Unitask.Application.Common.Settings;

public class JwtSettings
{
    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public string Secret { get; set; } = null!;

    public int ExpiryMinutes { get; set; }

    public int RefreshExpiryDays { get; set; }
}
