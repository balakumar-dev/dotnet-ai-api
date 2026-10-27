using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Interfaces;

public interface IRagChatService
{
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
