namespace DotNetAI.Core.Interfaces;

public interface IDocumentTextExtractor
{
    Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
