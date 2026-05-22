using Unitask.Api.Services;
using Unitask.Api.Models;

namespace Unitask.Api.Extensions;

public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký Normalization Service
        services.AddScoped<IDataNormalizationService, DataNormalizationService>();

        // Đăng ký Embedding Service - chọn implementation tùy theo config
        var ragConfig = configuration.GetSection("RAG").Get<RagConfig>();
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
            // Mặc định dùng Mock cho development
            services.AddScoped<IEmbeddingService>(sp => 
                new MockEmbeddingService(ragConfig?.Qdrant?.VectorSize ?? 384));
        }

        // Đăng ký Qdrant Service
        services.AddHttpClient<QdrantService>();
        services.AddScoped<IQdrantService>(sp => sp.GetRequiredService<QdrantService>());

        // Đăng ký RAG Service chính
        services.AddHttpClient<RagService>();
        services.AddScoped<IRagService>(sp => sp.GetRequiredService<RagService>());

        // AI matching service cho website
        services.AddScoped<IAiMatchingService, AiMatchingService>();

        services.AddHttpClient<CareerAssistantService>();
        services.AddScoped<ICareerAssistantService>(sp => sp.GetRequiredService<CareerAssistantService>());

        return services;
    }
}
