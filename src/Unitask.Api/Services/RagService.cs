using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Unitask.Infrastructure.Persistence;
using Unitask.Domain.Entities;
using Unitask.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Unitask.Api.Services;

/// <summary>
/// Main RAG Service - kết nối tất cả các components
/// </summary>
public interface IRagService
{
    Task IndexJobsAsync();
    Task IndexUsersAsync();
    Task IndexApplicationsAsync();
    Task IndexTaxonomyAsync();
    Task<RagQueryResponse> QueryAsync(RagQueryRequest request);
    Task<List<RagSearchResult>> SearchSimilarAsync(string query, int topK = 5, string? documentType = null);
    Task RefreshIndexAsync();
    Task<bool> HealthCheckAsync();
}

public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IQdrantService _qdrantService;
    private readonly IDataNormalizationService _normalizationService;
    private readonly UnitaskDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RagService> _logger;
    private readonly HttpClient _httpClient;

    public RagService(
        IEmbeddingService embeddingService,
        IQdrantService qdrantService,
        IDataNormalizationService normalizationService,
        UnitaskDbContext dbContext,
        IConfiguration configuration,
        ILogger<RagService> logger,
        HttpClient httpClient)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _normalizationService = normalizationService;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task IndexJobsAsync()
    {
        try
        {
            _logger.LogInformation("Starting to index jobs...");
            var jobs = await _dbContext.Jobs.ToListAsync();
            var documents = jobs.Select(j => _normalizationService.NormalizeJob(j)).ToList();
            
            await IndexDocumentsAsync(documents, "jobs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing jobs");
            throw;
        }
    }

    public async Task IndexUsersAsync()
    {
        try
        {
            _logger.LogInformation("Starting to index users...");
            var users = await _dbContext.Users.ToListAsync();
            var documents = users.Select(u => _normalizationService.NormalizeUser(u)).ToList();
            
            await IndexDocumentsAsync(documents, "users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing users");
            throw;
        }
    }

    public async Task IndexApplicationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting to index applications...");
            var applications = await _dbContext.JobApplications.Include(a => a.Job).ToListAsync();
            var documents = applications
                .Select(a => _normalizationService.NormalizeApplication(a, a.Job))
                .ToList();
            
            await IndexDocumentsAsync(documents, "applications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing applications");
            throw;
        }
    }

    public async Task<RagQueryResponse> QueryAsync(RagQueryRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Bước 1: Normalize query
            var normalizedQuery = _normalizationService.NormalizeText(request.Query);
            
            // Bước 2: Tạo embedding cho query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(normalizedQuery);
            
            // Bước 3: Tìm kiếm trong Qdrant
            var searchResults = await _qdrantService.SearchAsync(queryEmbedding, request.TopK, request.DocumentType);
            
            // Bước 4: Tạo context từ kết quả
            var context = FormatContext(searchResults);
            
            // Bước 5: Gọi LLM để tạo response
            string? llmResponse = null;
            if (searchResults.Any())
            {
                llmResponse = await GenerateLLMResponseAsync(request.Query, context);
            }
            
            stopwatch.Stop();
            
            return new RagQueryResponse
            {
                Query = request.Query,
                Results = searchResults,
                LLMResponse = llmResponse,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RAG query");
            stopwatch.Stop();
            
            return new RagQueryResponse
            {
                Query = request.Query,
                Results = new List<RagSearchResult>(),
                LLMResponse = $"Lỗi xử lý truy vấn: {ex.Message}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<List<RagSearchResult>> SearchSimilarAsync(string query, int topK = 5, string? documentType = null)
    {
        try
        {
            var normalizedQuery = _normalizationService.NormalizeText(query);
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(normalizedQuery);
            return await _qdrantService.SearchAsync(queryEmbedding, topK, documentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching similar documents");
            return new List<RagSearchResult>();
        }
    }

    public async Task RefreshIndexAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing RAG index...");
            
            // Xóa collection cũ
            await _qdrantService.DeleteCollectionAsync();
            
            // Tạo lại từ đầu (chạy tuần tự để tránh đa luồng dùng cùng DbContext)
            await IndexJobsAsync();
            await IndexUsersAsync();
            await IndexApplicationsAsync();
            await IndexTaxonomyAsync();
            
            _logger.LogInformation("RAG index refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing index");
            throw;
        }
    }

    public async Task IndexTaxonomyAsync()
    {
        try
        {
            _logger.LogInformation("Indexing taxonomy items...");
            var docs = _normalizationService.GetTaxonomyDocuments();
            if (docs == null || !docs.Any())
            {
                _logger.LogInformation("No taxonomy items found to index");
                return;
            }

            await IndexDocumentsAsync(docs, "taxonomy");
            _logger.LogInformation($"Indexed {docs.Count} taxonomy items");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing taxonomy");
            throw;
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            return await _qdrantService.HealthCheckAsync();
        }
        catch
        {
            return false;
        }
    }

    private async Task IndexDocumentsAsync(List<RagDocument> documents, string batchName)
    {
        _logger.LogInformation($"Indexing {documents.Count} {batchName}...");
        
        var batchSize = 10;
        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var batch = documents.Skip(i).Take(batchSize).ToList();
            var embeddings = await _embeddingService.GetEmbeddingsAsync(
                batch.Select(d => d.Content).ToList()
            );

            for (int j = 0; j < batch.Count; j++)
            {
                try
                {
                    await _qdrantService.UpsertDocumentAsync(batch[j], embeddings[j]);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error indexing document {batch[j].Id}");
                }
            }

            _logger.LogInformation($"Indexed {Math.Min(i + batchSize, documents.Count)}/{documents.Count} {batchName}");
        }
    }

    private string FormatContext(List<RagSearchResult> results)
    {
        if (!results.Any())
            return string.Empty;

        var context = "Thông tin liên quan:\n\n";
        foreach (var result in results.OrderByDescending(r => r.Score))
        {
            context += $"📌 {result.Title} (Độ phù hợp: {(result.Score * 100):F1}%)\n";
            context += $"{result.Content.Substring(0, Math.Min(200, result.Content.Length))}...\n\n";
        }

        return context;
    }

    private async Task<string> GenerateLLMResponseAsync(string query, string context)
    {
        try
        {
            var ragConfig = _configuration.GetSection("RAG").Get<RagConfig>();
            var useLLM = ragConfig?.UseLLM ?? "Groq";

            return useLLM.ToLower() switch
            {
                "groq" => await GenerateGroqResponseAsync(query, context, ragConfig?.Groq),
                "ollama" => await GenerateOllamaResponseAsync(query, context, ragConfig?.Ollama),
                _ => $"Không thể xác định LLM: {useLLM}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM response");
            return $"Lỗi tạo phản hồi: {ex.Message}";
        }
    }

    private async Task<string> GenerateGroqResponseAsync(string query, string context, GroqConfig? config)
    {
        if (config == null)
            return "Groq config not found";

        try
        {
            var messages = new[]
            {
                new { role = "system", content = "Bạn là một trợ lý thông minh giúp trả lời câu hỏi dựa trên thông tin được cung cấp. Hãy trả lời bằng tiếng Việt." },
                new { role = "user", content = $"Context:\n{context}\n\nCâu hỏi: {query}" }
            };

            var request = new { model = config.Model, messages = messages, temperature = 0.7, max_tokens = 500 };
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, config.Endpoint)
            {
                Content = JsonContent.Create(request)
            };
            
            httpRequest.Headers.Add("Authorization", $"Bearer {config.ApiKey}");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var content = json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? "Không có phản hồi";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Groq response");
            throw;
        }
    }

    private async Task<string> GenerateOllamaResponseAsync(string query, string context, OllamaConfig? config)
    {
        if (config == null)
            return "Ollama config not found";

        try
        {
            var prompt = $"Context:\n{context}\n\nCâu hỏi: {query}\n\nTrả lời:";
            var request = new { model = config.Model, prompt = prompt, stream = false, temperature = 0.7 };

            var response = await _httpClient.PostAsJsonAsync($"{config.Endpoint}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var result = json.GetProperty("response").GetString();

            return result ?? "Không có phản hồi";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Ollama response");
            throw;
        }
    }
}
