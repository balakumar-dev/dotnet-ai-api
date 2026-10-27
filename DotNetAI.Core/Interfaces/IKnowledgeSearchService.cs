using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Interfaces;

public interface IKnowledgeSearchService
{
    Task<KnowledgeSearchResponse> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default);
}
