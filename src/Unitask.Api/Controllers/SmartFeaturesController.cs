using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs.Insights;

namespace Unitask.Api.Controllers;

[ApiController]
public class SmartFeaturesController : ControllerBase
{
    private readonly IInsightsService _insightsService;

    public SmartFeaturesController(IInsightsService insightsService)
    {
        _insightsService = insightsService;
    }

    [HttpPost("/api/matching/recommendations")]
    public async Task<ActionResult<JobRecommendationResponse>> GetRecommendations([FromBody] JobRecommendationRequest request, CancellationToken cancellationToken)
    {
        var response = await _insightsService.GetRecommendationsAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("/api/career-assistant/chat")]
    public async Task<ActionResult<CareerChatResponse>> Chat([FromBody] CareerChatRequest request, CancellationToken cancellationToken)
    {
        var response = await _insightsService.ChatAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("/api/personalization/{userId:guid}")]
    public async Task<ActionResult<PersonalizationResponse>> GetPersonalization([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _insightsService.GetPersonalizationAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("/api/automation/suggestions")]
    public async Task<ActionResult<AutomationSuggestionResponse>> GetAutomationSuggestions(
        [FromQuery] Guid? businessId,
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _insightsService.GetAutomationSuggestionsAsync(businessId, userId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}