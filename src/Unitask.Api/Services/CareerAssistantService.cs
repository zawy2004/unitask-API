using Unitask.Api.Models;
using Unitask.Application.DTOs.Insights;

namespace Unitask.Api.Services;

public interface ICareerAssistantService
{
    Task<CareerChatResponse> ChatAsync(CareerChatRequest request, CancellationToken cancellationToken = default);
}

public class CareerAssistantService : ICareerAssistantService
{
    private readonly IRagService _ragService;

    public CareerAssistantService(IRagService ragService)
    {
        _ragService = ragService;
    }

    public async Task<CareerChatResponse> ChatAsync(CareerChatRequest request, CancellationToken cancellationToken = default)
    {
        // Use RAG to fetch related docs and generate reply
        var queryResponse = await _ragService.QueryAsync(new RagQueryRequest { Query = request.Message, TopK = Math.Max(1, request.TopK) });
        return new CareerChatResponse
        {
            Reply = queryResponse.LLMResponse ?? string.Empty,
            Jobs = new List<InsightJobCardDto>(),
            FollowUpQuestions = new List<string>(),
            CareerPaths = new List<string>(),
            Refused = false,
            Summary = queryResponse.Results.Count > 0 ? "Gợi ý dựa trên dữ liệu" : "Không có dữ liệu phù hợp"
        };
    }
}
