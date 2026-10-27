using DotNetAI.Core.DTOs;
using DotNetAI.Core.Prompts;

namespace DotNetAI.Core.Interfaces;

public interface IAIService
{
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);

    Task<TopicAnalysis> AnalyzeTopicAsync(ChatRequest request, CancellationToken cancellationToken = default);

    Task<ChatResponse> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken = default);
}
