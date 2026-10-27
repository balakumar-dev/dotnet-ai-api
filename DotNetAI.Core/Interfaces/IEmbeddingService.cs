using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(
        string text,
        EmbeddingPurpose purpose,
        CancellationToken cancellationToken = default);
}
