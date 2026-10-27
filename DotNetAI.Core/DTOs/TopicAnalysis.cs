namespace DotNetAI.Core.DTOs;

public sealed class TopicAnalysis
{
    public string Summary { get; set; } = string.Empty;

    public List<string> Technologies { get; set; } = [];

    public string Difficulty { get; set; } = string.Empty;
}
