namespace Unitask.Api.Models;

public class RagConfig
{
    public QdrantConfig? Qdrant { get; set; }
    public GroqConfig? Groq { get; set; }
    public OllamaConfig? Ollama { get; set; }
    public EmbeddingConfig? Embedding { get; set; }
    public string UseLLM { get; set; } = "Groq";
}
