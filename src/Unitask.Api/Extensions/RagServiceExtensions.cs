using Unitask.Api.Jobs;
using Unitask.Api.Services;
using Unitask.Api.Models;

namespace Unitask.Api.Extensions;

public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDataNormalizationService, DataNormalizationService>();

        var ragConfig = configuration.GetSection("RAG").Get<RagConfig>();
        var groqApiKey = configuration.GetValue<string>("RAG:Groq:ApiKey") ?? ragConfig?.Groq?.ApiKey;

        if (!string.IsNullOrWhiteSpace(groqApiKey))
        {
            services.AddHttpClient<GroqEmbeddingService>();
            services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<GroqEmbeddingService>());
        }
        else
        {
            var embeddingType = configuration.GetValue<string>("RAG:Embedding:ModelPath") ?? string.Empty;

            if (embeddingType.Contains("sentence-transformer", StringComparison.OrdinalIgnoreCase))
            {
                services.AddHttpClient<SentenceTransformerEmbeddingService>();
                services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<SentenceTransformerEmbeddingService>());
            }
            else if (embeddingType.Contains("ollama", StringComparison.OrdinalIgnoreCase) ||
                     ragConfig?.UseLLM == "Ollama")
            {
                services.AddHttpClient<OllamaEmbeddingService>();
                services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<OllamaEmbeddingService>());
            }
            else
            {
                services.AddScoped<IEmbeddingService>(sp =>
                    new MockEmbeddingService(ragConfig?.Qdrant?.VectorSize ?? 384));
            }
        }

        services.AddHttpClient<QdrantService>();
        services.AddScoped<IQdrantService>(sp => sp.GetRequiredService<QdrantService>());

        services.AddHttpClient<RagService>();
        services.AddScoped<IRagService>(sp => sp.GetRequiredService<RagService>());

        services.AddScoped<IAiMatchingService, AiMatchingService>();

        services.AddHttpClient<CareerAssistantService>();
        services.AddScoped<ICareerAssistantService>(sp => sp.GetRequiredService<CareerAssistantService>());

        // Auto-index every 6 hours
        services.AddHostedService<RagAutoIndexService>();

        return services;
    }
}
