using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;

namespace DotNetAI.Infrastructure.AI;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly List<KnowledgeChunk> _chunks = [];
    private readonly object _sync = new();

    public int Count
    {
        get
        {
            lock (_sync)
            {
                return _chunks.Count;
            }
        }
    }

    public Task UpsertAsync(KnowledgeChunk chunk, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        lock (_sync)
        {
            _chunks.RemoveAll(existing => existing.ChunkId == chunk.ChunkId);
            _chunks.Add(chunk);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeSearchMatch>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);

        List<KnowledgeSearchMatch> matches;
        lock (_sync)
        {
            matches = _chunks
                .Select(chunk => new KnowledgeSearchMatch
                {
                    Document = chunk.FileName,
                    ChunkId = chunk.ChunkId,
                    Content = chunk.Content,
                    Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
                })
                .OrderByDescending(match => match.Score)
                .Take(topK)
                .ToList();
        }

        return Task.FromResult<IReadOnlyList<KnowledgeSearchMatch>>(matches);
    }

    public Task DeleteByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        lock (_sync)
        {
            _chunks.RemoveAll(chunk => chunk.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<KnowledgeDocumentInfo> GetDocuments()
    {
        lock (_sync)
        {
            return _chunks
                .GroupBy(chunk => chunk.DocumentId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new KnowledgeDocumentInfo
                {
                    DocumentId = group.Key,
                    FileName = group.First().FileName,
                    ChunkCount = group.Count()
                })
                .OrderBy(document => document.FileName)
                .ToList();
        }
    }

    private static float CosineSimilarity(float[] left, float[] right)
    {
        if (left.Length == 0 || right.Length == 0 || left.Length != right.Length)
        {
            return 0f;
        }

        float dot = 0f;
        float leftMagnitude = 0f;
        float rightMagnitude = 0f;

        for (int i = 0; i < left.Length; i++)
        {
            dot += left[i] * right[i];
            leftMagnitude += left[i] * left[i];
            rightMagnitude += right[i] * right[i];
        }

        float denominator = MathF.Sqrt(leftMagnitude) * MathF.Sqrt(rightMagnitude);
        return denominator == 0f ? 0f : dot / denominator;
    }
}
