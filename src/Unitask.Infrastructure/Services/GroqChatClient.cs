using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Unitask.Infrastructure.Services;

public interface IGroqChatClient
{
    Task<string?> ChatAsync(string systemPrompt, IReadOnlyList<(string Role, string Content)> messages, CancellationToken cancellationToken = default);
}

public class GroqChatClient : IGroqChatClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GroqChatClient> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _endpoint;
    private readonly bool _enabled;

    public GroqChatClient(HttpClient http, IConfiguration config, ILogger<GroqChatClient> logger)
    {
        _http = http;
        _logger = logger;
        _apiKey = config["RAG:Groq:ApiKey"] ?? string.Empty;
        _model = config["RAG:Groq:Model"] ?? "llama-3.3-70b-versatile";
        _endpoint = config["RAG:Groq:Endpoint"] ?? "https://api.groq.com/openai/v1/chat/completions";
        _enabled = !string.IsNullOrWhiteSpace(_apiKey);

        if (_enabled)
        {
            _http.Timeout = TimeSpan.FromSeconds(20);
        }
    }

    public async Task<string?> ChatAsync(string systemPrompt, IReadOnlyList<(string Role, string Content)> messages, CancellationToken cancellationToken = default)
    {
        if (!_enabled)
        {
            return null;
        }

        var payloadMessages = new List<object> { new { role = "system", content = systemPrompt } };
        foreach (var (role, content) in messages)
        {
            if (string.IsNullOrWhiteSpace(content)) continue;
            payloadMessages.Add(new { role, content });
        }

        var payload = new
        {
            model = _model,
            messages = payloadMessages,
            temperature = 0.5,
            max_tokens = 700,
            top_p = 0.9
        };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Groq returned {Status}: {Body}", resp.StatusCode, errorBody.Length > 500 ? errorBody[..500] : errorBody);
                return null;
            }

            var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return reply?.Trim();
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Groq call timed out after 20s");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Groq call failed");
            return null;
        }
    }
}
