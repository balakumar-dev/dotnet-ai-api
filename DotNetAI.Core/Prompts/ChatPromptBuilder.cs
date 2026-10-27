using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Prompts;

public sealed class ChatPromptBuilder : IChatPromptBuilder
{
    public ChatPrompt Build(string userMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        return new ChatPrompt
        {
            SystemInstruction = PromptTemplates.DotNetAssistantSystem,
            UserMessage = PromptTemplates.CreateUserPrompt(userMessage)
        };
    }

    public ChatPrompt BuildTopicAnalysis(string userMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        return new ChatPrompt
        {
            SystemInstruction = PromptTemplates.TopicAnalysisSystem,
            UserMessage = PromptTemplates.CreateTopicAnalysisPrompt(userMessage)
        };
    }

    public ChatPrompt BuildRag(string question, IReadOnlyList<KnowledgeSearchMatch> matches)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentNullException.ThrowIfNull(matches);

        string context = string.Join(
            Environment.NewLine + Environment.NewLine,
            matches.Select(match =>
                $"[{match.ChunkId} from {match.Document}]\n{match.Content}"));

        return new ChatPrompt
        {
            SystemInstruction = PromptTemplates.RagSystem,
            UserMessage = PromptTemplates.CreateRagUserPrompt(question, context)
        };
    }
}
