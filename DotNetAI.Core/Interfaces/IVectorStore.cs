using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Interfaces;

public interface IVectorStore
{
    Task UpsertAsync(KnowledgeChunk chunk, CancellationToken cancellationToken = default);

    Task DeleteByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeSearchMatch>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);

    IReadOnlyList<KnowledgeDocumentInfo> GetDocuments();

    int Count { get; }
}
