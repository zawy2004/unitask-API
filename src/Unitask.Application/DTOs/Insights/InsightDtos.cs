using System;
using System.Collections.Generic;

namespace Unitask.Application.DTOs.Insights;

public class JobRecommendationRequest
{
    public string? Query { get; set; }

    public string? Role { get; set; }

    public string? Major { get; set; }

    public IReadOnlyList<string>? Skills { get; set; }

    public string? Bio { get; set; }

    public string? CompanyName { get; set; }

    public string? University { get; set; }

    public string? Location { get; set; }

    public int TopK { get; set; } = 6;
}

public class CareerChatTurnDto
{
    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;
}

public class CareerUserContextDto
{
    public string? Role { get; set; }

    public string? Major { get; set; }

    public string? University { get; set; }

    public string? CompanyName { get; set; }

    public IReadOnlyList<string>? Skills { get; set; }

    public string? Bio { get; set; }
}

public class CareerChatRequest
{
    public string Message { get; set; } = null!;

    public CareerUserContextDto? User { get; set; }

    public IReadOnlyList<CareerChatTurnDto> History { get; set; } = Array.Empty<CareerChatTurnDto>();

    public int TopK { get; set; } = 5;
}

public class InsightTagDto
{
    public string Label { get; set; } = null!;

    public string Variant { get; set; } = "p";
}

public class InsightJobCardDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Title { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string LogoText { get; set; } = null!;

    public string LogoGradient { get; set; } = null!;

    public bool Verified { get; set; }

    public string Location { get; set; } = null!;

    public IReadOnlyList<InsightTagDto> Tags { get; set; } = Array.Empty<InsightTagDto>();

    public int SpotsLeft { get; set; }

    public int SpotsTotal { get; set; }

    public string Pay { get; set; } = null!;

    public decimal? PayMin { get; set; }

    public decimal? PayMax { get; set; }

    public string Deadline { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool? Featured { get; set; }

    public string Description { get; set; } = null!;

    public IReadOnlyList<string> Requirements { get; set; } = Array.Empty<string>();

    public string Duration { get; set; } = "N/A";

    public string PostedAt { get; set; } = "N/A";

    public decimal MatchScore { get; set; }

    public IReadOnlyList<string> MatchReasons { get; set; } = Array.Empty<string>();
}

public class JobRecommendationResponse
{
    public string Query { get; set; } = string.Empty;

    public bool UsedSemanticSearch { get; set; }

    public string Summary { get; set; } = string.Empty;

    public IReadOnlyList<InsightJobCardDto> Matches { get; set; } = Array.Empty<InsightJobCardDto>();
}

public class CareerChatResponse
{
    public string Reply { get; set; } = string.Empty;

    public IReadOnlyList<InsightJobCardDto> Jobs { get; set; } = Array.Empty<InsightJobCardDto>();

    public IReadOnlyList<string> FollowUpQuestions { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> CareerPaths { get; set; } = Array.Empty<string>();

    public bool Refused { get; set; }

    public string? Summary { get; set; }
}

public class PersonalizationResponse
{
    public Guid UserId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public IReadOnlyList<string> RecommendedKeywords { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> RecommendedCategories { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> RecommendedLocations { get; set; } = Array.Empty<string>();

    public IReadOnlyList<InsightJobCardDto> SuggestedJobs { get; set; } = Array.Empty<InsightJobCardDto>();

    public IReadOnlyList<string> NextActions { get; set; } = Array.Empty<string>();
}

public class AutomationSuggestionDto
{
    public string Id { get; set; } = null!;

    public string Icon { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Benefit { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? ActionLink { get; set; }

    public string Priority { get; set; } = "normal";
}

public class AutomationSuggestionResponse
{
    public Guid? BusinessId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public int PendingApplications { get; set; }

    public IReadOnlyList<AutomationSuggestionDto> Suggestions { get; set; } = Array.Empty<AutomationSuggestionDto>();
}