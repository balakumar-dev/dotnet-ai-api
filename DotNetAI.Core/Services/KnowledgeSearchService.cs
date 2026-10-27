using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;

namespace DotNetAI.Core.Services;

public sealed class KnowledgeSearchService : IKnowledgeSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public KnowledgeSearchService(IEmbeddingService embeddingService, IVectorStore vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<KnowledgeSearchResponse> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(request));
        }

        int topK = request.TopK <= 0 ? 3 : Math.Min(request.TopK, 10);

        if (_vectorStore.Count == 0)
        {
            throw new InvalidOperationException(
                "Knowledge base is empty. Gemini embedding quota may have been exceeded during startup. Wait about a minute, restart the API, and look for 'Knowledge base ready' in the logs.");
        }

        float[] queryEmbedding = await _embeddingService.EmbedAsync(
            request.Query,
            EmbeddingPurpose.Query,
            cancellationToken);

        IReadOnlyList<KnowledgeSearchMatch> matches = await _vectorStore.SearchAsync(
            queryEmbedding,
            topK,
            cancellationToken);

        return new KnowledgeSearchResponse
        {
            Results = matches
        };
    }
}
