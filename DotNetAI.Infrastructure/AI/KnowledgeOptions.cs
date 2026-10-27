namespace DotNetAI.Infrastructure.AI;

public sealed class KnowledgeOptions
{
    public const string SectionName = "Knowledge";

    public string DocumentsPath { get; set; } = "Knowledge";

    public int ChunkSize { get; set; } = 2000;

    public int ChunkOverlap { get; set; } = 80;

    public int EmbeddingDelayMilliseconds { get; set; } = 4000;

    public long MaxUploadBytes { get; set; } = 2 * 1024 * 1024;

    public int RagTopK { get; set; } = 3;

    public float MinimumSimilarityScore { get; set; } = 0.45f;
}
