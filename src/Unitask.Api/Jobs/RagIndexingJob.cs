using Unitask.Api.Services;

namespace Unitask.Api.Jobs;

public class RagIndexingJob
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagIndexingJob> _logger;

    public RagIndexingJob(IRagService ragService, ILogger<RagIndexingJob> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting RAG indexing job...");
            await _ragService.RefreshIndexAsync();
            _logger.LogInformation("RAG indexing job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG indexing job failed");
        }
    }
}
