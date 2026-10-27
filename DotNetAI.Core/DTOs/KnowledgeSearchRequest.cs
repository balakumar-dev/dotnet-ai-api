namespace DotNetAI.Core.DTOs;

public sealed class KnowledgeSearchRequest
{
    public string Query { get; set; } = string.Empty;

    public int TopK { get; set; } = 3;
}
