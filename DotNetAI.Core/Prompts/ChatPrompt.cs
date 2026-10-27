namespace DotNetAI.Core.Prompts;

public sealed class ChatPrompt
{
    public required string SystemInstruction { get; init; }

    public required string UserMessage { get; init; }
}
