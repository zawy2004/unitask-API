using Unitask.Api.Services;

namespace Unitask.Api.Jobs;

public class RagAutoIndexService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RagAutoIndexService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    public RagAutoIndexService(IServiceScopeFactory scopeFactory, ILogger<RagAutoIndexService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RAG Auto-Index Service started. First run in {Delay}", StartupDelay);
        await Task.Delay(StartupDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ragService = scope.ServiceProvider.GetRequiredService<IRagService>();

                var healthy = await ragService.HealthCheckAsync();
                if (!healthy)
                {
                    _logger.LogWarning("Qdrant not reachable, skipping index cycle");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Starting scheduled RAG index refresh...");
                await ragService.RefreshIndexAsync();
                _logger.LogInformation("Scheduled RAG index refresh completed");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RAG auto-index cycle failed, retrying next interval");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("RAG Auto-Index Service stopped");
    }
}
