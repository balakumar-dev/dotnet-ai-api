using System.Text;
using DotNetAI.Core.Interfaces;
using UglyToad.PdfPig;

namespace DotNetAI.Infrastructure.Documents;

public sealed class PdfDocumentExtractor : IFileFormatExtractor
{
    public bool CanExtract(string fileName)
    {
        return Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> ExtractAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document = PdfDocument.Open(stream);
        var text = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            text.AppendLine(page.Text);
        }

        return Task.FromResult(text.ToString());
    }
}
