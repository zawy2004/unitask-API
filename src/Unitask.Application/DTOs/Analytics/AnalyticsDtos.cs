using System.Collections.Generic;

namespace Unitask.Application.DTOs.Analytics;

public class AnalyticsDailyPointDto
{
    public string Date { get; set; } = string.Empty;

    public int Users { get; set; }

    public int Sessions { get; set; }

    public int PageViews { get; set; }
}

public class AnalyticsTopPageDto
{
    public string Path { get; set; } = string.Empty;

    public int Views { get; set; }
}

public class AnalyticsTrafficSourceDto
{
    public string Source { get; set; } = string.Empty;

    public int Sessions { get; set; }
}

public class AnalyticsOverviewDto
{
    /// <summary>False khi chưa cấu hình GoogleAnalytics:PropertyId / ServiceAccountJson.</summary>
    public bool Configured { get; set; } = true;

    public int ActiveUsersNow { get; set; }

    public int TotalUsers { get; set; }

    public int NewUsers { get; set; }

    public int Sessions { get; set; }

    public int PageViews { get; set; }

    public double AvgSessionDurationSeconds { get; set; }

    public double BounceRate { get; set; }

    public IReadOnlyList<AnalyticsDailyPointDto> DailySeries { get; set; } = new List<AnalyticsDailyPointDto>();

    public IReadOnlyList<AnalyticsTopPageDto> TopPages { get; set; } = new List<AnalyticsTopPageDto>();

    public IReadOnlyList<AnalyticsTrafficSourceDto> TrafficSources { get; set; } = new List<AnalyticsTrafficSourceDto>();
}
