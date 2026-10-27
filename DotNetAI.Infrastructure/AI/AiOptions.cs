namespace DotNetAI.Infrastructure.AI;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Gemini";

    public int MaxOutputTokens { get; set; } = 400;

    public float Temperature { get; set; } = 0.3f;
}
