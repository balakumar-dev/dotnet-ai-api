namespace DotNetAI.Core.DTOs;

public sealed class KnowledgeDocumentInfo
{
    public string DocumentId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int ChunkCount { get; set; }
}
