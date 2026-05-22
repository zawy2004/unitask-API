namespace Unitask.Api.Models;

/// <summary>
/// Dữ liệu chuẩn hóa cho RAG
/// </summary>
public class RagDocument
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;  // "job", "user", "application"
    public string Content { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Vector embedding kết quả
/// </summary>
public class VectorEmbedding
{
    public string DocumentId { get; set; } = string.Empty;
    public float[] Vector { get; set; } = Array.Empty<float>();
    public RagDocument Document { get; set; } = new();
}

/// <summary>
/// Kết quả tìm kiếm RAG
/// </summary>
public class RagSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// RAG Query Request
/// </summary>
public class RagQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
    public string? DocumentType { get; set; }  // Filter by type
}

/// <summary>
/// RAG Query Response
/// </summary>
public class RagQueryResponse
{
    public string Query { get; set; } = string.Empty;
    public List<RagSearchResult> Results { get; set; } = new();
    public string? LLMResponse { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Cấu hình Qdrant
/// </summary>
public class QdrantConfig
{
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "unitask_knowledge";
    public int VectorSize { get; set; } = 384;
}

/// <summary>
/// Cấu hình LLM
/// </summary>
public class LLMConfig
{
    public GroqConfig? Groq { get; set; }
    public OllamaConfig? Ollama { get; set; }
    public EmbeddingConfig? Embedding { get; set; }
    public string UseLLM { get; set; } = "Groq";
}

public class GroqConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string Endpoint { get; set; } = string.Empty;
}

public class OllamaConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = "llama3.1:8b";
}

public class EmbeddingConfig
{
    public string ModelPath { get; set; } = string.Empty;
    public int Dimension { get; set; } = 384;
}
