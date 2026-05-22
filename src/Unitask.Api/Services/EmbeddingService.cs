using System.Net.Http.Json;
using System.Text.Json;
using Unitask.Api.Models;

namespace Unitask.Api.Services;

/// <summary>
/// Service tạo embeddings từ text
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<List<float[]>> GetEmbeddingsAsync(List<string> texts);
}

/// <summary>
/// Ollama Embedding Service - sử dụng model local
/// </summary>
public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(HttpClient httpClient, IConfiguration config, ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var ragConfig = config.GetSection("RAG").Get<RagConfig>();
        _endpoint = ragConfig?.Ollama?.Endpoint ?? "http://localhost:11434";
        _model = ragConfig?.Ollama?.Model ?? "llama3.1:8b";
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        try
        {
            var request = new { prompt = text, model = _model };
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/api/embeddings", request);
            response.EnsureSuccessStatusCode();

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var embedding = json.GetProperty("embedding");
            
            return embedding.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embedding from Ollama");
            throw;
        }
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
    {
        var tasks = texts.Select(t => GetEmbeddingAsync(t)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}

/// <summary>
/// Sentence Transformer Embedding Service - sử dụng model "all-MiniLM-L6-v2"
/// </summary>
public class SentenceTransformerEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly ILogger<SentenceTransformerEmbeddingService> _logger;

    public SentenceTransformerEmbeddingService(HttpClient httpClient, IConfiguration config, ILogger<SentenceTransformerEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Mặc định sử dụng local sentence-transformer API
        var ragConfig = config.GetSection("RAG").Get<RagConfig>();
        _endpoint = ragConfig?.Embedding?.ModelPath ?? "http://localhost:8000";
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        try
        {
            var request = new { texts = new[] { text } };
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/embed", request);
            response.EnsureSuccessStatusCode();

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var embeddings = json.GetProperty("embeddings");
            var firstEmbedding = embeddings[0];
            
            return firstEmbedding.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embedding from Sentence Transformer");
            throw;
        }
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
    {
        try
        {
            var request = new { texts };
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/embed", request);
            response.EnsureSuccessStatusCode();

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var embeddings = json.GetProperty("embeddings");
            
            var result = new List<float[]>();
            foreach (var embedding in embeddings.EnumerateArray())
            {
                result.Add(embedding.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray());
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embeddings from Sentence Transformer");
            throw;
        }
    }
}

/// <summary>
/// Mock Embedding Service - cho development/testing
/// </summary>
public class MockEmbeddingService : IEmbeddingService
{
    private readonly int _dimension;
    private readonly Random _random = new(42);

    public MockEmbeddingService(int dimension = 384)
    {
        _dimension = dimension;
    }

    public Task<float[]> GetEmbeddingAsync(string text)
    {
        // Tạo vector ngẫu nhiên nhưng consistent cho cùng text
        var hash = text.GetHashCode();
        var rng = new Random(hash);
        var vector = new float[_dimension];
        for (int i = 0; i < _dimension; i++)
        {
            vector[i] = (float)rng.NextDouble();
        }
        
        return Task.FromResult(vector);
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
    {
        var tasks = texts.Select(t => GetEmbeddingAsync(t)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
