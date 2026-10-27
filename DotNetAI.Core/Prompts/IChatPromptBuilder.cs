using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Prompts;

public interface IChatPromptBuilder
{
    ChatPrompt Build(string userMessage);

    ChatPrompt BuildTopicAnalysis(string userMessage);

    ChatPrompt BuildRag(string question, IReadOnlyList<KnowledgeSearchMatch> matches);
}
