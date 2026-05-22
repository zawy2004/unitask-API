using System;
using System.Threading;
using System.Threading.Tasks;
using Unitask.Application.DTOs.Insights;

namespace Unitask.Application.Common.Interfaces;

public interface IInsightsService
{
    Task<JobRecommendationResponse> GetRecommendationsAsync(JobRecommendationRequest request, CancellationToken cancellationToken = default);

    Task<CareerChatResponse> ChatAsync(CareerChatRequest request, CancellationToken cancellationToken = default);

    Task<PersonalizationResponse> GetPersonalizationAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AutomationSuggestionResponse> GetAutomationSuggestionsAsync(Guid? businessId, Guid? userId, CancellationToken cancellationToken = default);
}