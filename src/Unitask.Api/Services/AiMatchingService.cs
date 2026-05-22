using Unitask.Api.Models;
using Unitask.Infrastructure.Persistence;
using Unitask.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Unitask.Application.DTOs.Insights;

namespace Unitask.Api.Services;

public interface IAiMatchingService
{
    Task<JobRecommendationResponse> GetRecommendationsAsync(JobRecommendationRequest request, CancellationToken cancellationToken = default);
}

public class AiMatchingService : IAiMatchingService
{
    private readonly UnitaskDbContext _dbContext;

    public AiMatchingService(UnitaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JobRecommendationResponse> GetRecommendationsAsync(JobRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        // Simple wrapper that uses existing heuristics from the older InsightsService.
        // For now, delegate to database queries and simple scoring.
        var jobs = await _dbContext.Jobs.AsNoTracking().ToListAsync(cancellationToken);
        // Implement lightweight scoring or reuse existing logic from frontend if needed.
        return new JobRecommendationResponse
        {
            Query = request.Query ?? string.Empty,
            UsedSemanticSearch = false,
            Summary = "Gợi ý tạm thời - nâng cấp RAG để có kết quả tốt hơn.",
            Matches = new List<InsightJobCardDto>()
        };
    }
}
