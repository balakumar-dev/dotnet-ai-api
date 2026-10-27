using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Interfaces;

public interface IDocumentIngestionService
{
    Task<DocumentIngestionResult> IngestAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);
}
