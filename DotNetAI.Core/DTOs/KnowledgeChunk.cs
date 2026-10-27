namespace DotNetAI.Core.DTOs;

public sealed class KnowledgeChunk
{
    public string DocumentId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ChunkId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public float[] Embedding { get; set; } = [];

    public DateTimeOffset CreatedDate { get; set; }
}
