using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DotNetAI.Core.Interfaces;

namespace DotNetAI.Infrastructure.Documents;

public sealed class WordDocumentExtractor : IFileFormatExtractor
{
    public bool CanExtract(string fileName)
    {
        return Path.GetExtension(fileName).Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ExtractAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document = WordprocessingDocument.Open(stream, isEditable: false);
        Body? body = document.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return Task.FromResult(string.Empty);
        }

        IEnumerable<string> paragraphs = body
            .Elements<Paragraph>()
            .Select(paragraph => paragraph.InnerText);

        return Task.FromResult(string.Join(Environment.NewLine, paragraphs));
    }
}
