namespace DotNetAI.Core.DTOs;

public sealed class KnowledgeSearchResponse
{
    public IReadOnlyList<KnowledgeSearchMatch> Results { get; set; } = [];
}

public sealed class KnowledgeSearchMatch
{
    public string Document { get; set; } = string.Empty;

    public string ChunkId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public float Score { get; set; }
}
