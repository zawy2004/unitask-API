using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Unitask.Application.DTOs.Analytics;

namespace Unitask.Infrastructure.Services;

public class AnalyticsNotConfiguredException : Exception
{
}

public interface IGoogleAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(int days, CancellationToken cancellationToken = default);
}

/// <summary>
/// Đọc số liệu thật từ Google Analytics 4 (Data API) cho tổng quan admin.
/// Cấu hình qua GoogleAnalytics:PropertyId + GoogleAnalytics:ServiceAccountJson.
/// </summary>
public class GoogleAnalyticsService : IGoogleAnalyticsService
{
    private readonly string _propertyId;
    private readonly string _serviceAccountJson;
    private readonly Lazy<Task<BetaAnalyticsDataClient>> _client;

    public GoogleAnalyticsService(IConfiguration config)
    {
        _propertyId = config["GoogleAnalytics:PropertyId"] ?? string.Empty;
        _serviceAccountJson = config["GoogleAnalytics:ServiceAccountJson"] ?? string.Empty;
        _client = new Lazy<Task<BetaAnalyticsDataClient>>(BuildClientAsync);
    }

    private bool IsConfigured => !string.IsNullOrWhiteSpace(_propertyId) && !string.IsNullOrWhiteSpace(_serviceAccountJson);

    private Task<BetaAnalyticsDataClient> BuildClientAsync()
    {
        var builder = new BetaAnalyticsDataClientBuilder { GoogleCredential = GoogleCredential.FromJson(_serviceAccountJson) };
        return builder.BuildAsync();
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync(int days, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new AnalyticsNotConfiguredException();
        }

        var client = await _client.Value;
        var property = $"properties/{_propertyId}";
        var dateRange = new DateRange { StartDate = $"{days}daysAgo", EndDate = "today" };

        var totalsTask = client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Metrics =
            {
                new Metric { Name = "activeUsers" },
                new Metric { Name = "newUsers" },
                new Metric { Name = "sessions" },
                new Metric { Name = "screenPageViews" },
                new Metric { Name = "averageSessionDuration" },
                new Metric { Name = "bounceRate" },
            },
        }, cancellationToken);

        var dailyTask = client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "date" } },
            Metrics =
            {
                new Metric { Name = "activeUsers" },
                new Metric { Name = "sessions" },
                new Metric { Name = "screenPageViews" },
            },
            OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "date" } } },
        }, cancellationToken);

        var topPagesTask = client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "pagePath" } },
            Metrics = { new Metric { Name = "screenPageViews" } },
            OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "screenPageViews" }, Desc = true } },
            Limit = 10,
        }, cancellationToken);

        var sourcesTask = client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "sessionDefaultChannelGroup" } },
            Metrics = { new Metric { Name = "sessions" } },
            OrderBys = { new OrderBy { Metric = new OrderBy.Types.MetricOrderBy { MetricName = "sessions" }, Desc = true } },
            Limit = 8,
        }, cancellationToken);

        var realtimeTask = client.RunRealtimeReportAsync(new RunRealtimeReportRequest
        {
            Property = property,
            Metrics = { new Metric { Name = "activeUsers" } },
        }, cancellationToken);

        await Task.WhenAll(totalsTask, dailyTask, topPagesTask, sourcesTask, realtimeTask);

        var totals = await totalsTask;
        var daily = await dailyTask;
        var topPages = await topPagesTask;
        var sources = await sourcesTask;
        var realtime = await realtimeTask;

        var totalsRow = totals.Rows.FirstOrDefault();

        return new AnalyticsOverviewDto
        {
            Configured = true,
            ActiveUsersNow = ParseInt(realtime.Rows.FirstOrDefault()?.MetricValues.FirstOrDefault()?.Value),
            TotalUsers = ParseInt(totalsRow?.MetricValues.ElementAtOrDefault(0)?.Value),
            NewUsers = ParseInt(totalsRow?.MetricValues.ElementAtOrDefault(1)?.Value),
            Sessions = ParseInt(totalsRow?.MetricValues.ElementAtOrDefault(2)?.Value),
            PageViews = ParseInt(totalsRow?.MetricValues.ElementAtOrDefault(3)?.Value),
            AvgSessionDurationSeconds = ParseDouble(totalsRow?.MetricValues.ElementAtOrDefault(4)?.Value),
            BounceRate = ParseDouble(totalsRow?.MetricValues.ElementAtOrDefault(5)?.Value),
            DailySeries = daily.Rows.Select(r => new AnalyticsDailyPointDto
            {
                Date = FormatDate(r.DimensionValues.FirstOrDefault()?.Value),
                Users = ParseInt(r.MetricValues.ElementAtOrDefault(0)?.Value),
                Sessions = ParseInt(r.MetricValues.ElementAtOrDefault(1)?.Value),
                PageViews = ParseInt(r.MetricValues.ElementAtOrDefault(2)?.Value),
            }).ToList(),
            TopPages = topPages.Rows.Select(r => new AnalyticsTopPageDto
            {
                Path = r.DimensionValues.FirstOrDefault()?.Value ?? "/",
                Views = ParseInt(r.MetricValues.FirstOrDefault()?.Value),
            }).ToList(),
            TrafficSources = sources.Rows.Select(r => new AnalyticsTrafficSourceDto
            {
                Source = r.DimensionValues.FirstOrDefault()?.Value ?? "(khác)",
                Sessions = ParseInt(r.MetricValues.FirstOrDefault()?.Value),
            }).ToList(),
        };
    }

    private static int ParseInt(string? value) => int.TryParse(value, out var n) ? n : 0;

    private static double ParseDouble(string? value) => double.TryParse(value, out var n) ? n : 0;

    /// <summary>GA4 trả dimension "date" dạng "yyyyMMdd" -> chuẩn hoá "yyyy-MM-dd" cho biểu đồ.</summary>
    private static string FormatDate(string? raw)
    {
        if (string.IsNullOrEmpty(raw) || raw.Length != 8) return raw ?? string.Empty;
        return $"{raw[..4]}-{raw.Substring(4, 2)}-{raw.Substring(6, 2)}";
    }
}
