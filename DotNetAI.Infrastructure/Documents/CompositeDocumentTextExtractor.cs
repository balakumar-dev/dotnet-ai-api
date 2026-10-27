using DotNetAI.Core.Interfaces;

namespace DotNetAI.Infrastructure.Documents;

public sealed class CompositeDocumentTextExtractor : IDocumentTextExtractor
{
    private readonly IReadOnlyList<IFileFormatExtractor> _extractors;

    public CompositeDocumentTextExtractor(IEnumerable<IFileFormatExtractor> extractors)
    {
        _extractors = extractors.ToList();
    }

    public Task<string> ExtractAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        IFileFormatExtractor? extractor = _extractors.FirstOrDefault(item => item.CanExtract(fileName));
        if (extractor is null)
        {
            throw new ArgumentException(
                "Unsupported file type. Upload .txt, .md, .pdf, or .docx.",
                nameof(fileName));
        }

        return extractor.ExtractAsync(stream, fileName, cancellationToken);
    }
}
