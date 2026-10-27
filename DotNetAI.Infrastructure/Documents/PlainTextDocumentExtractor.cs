using DotNetAI.Core.Interfaces;

namespace DotNetAI.Infrastructure.Documents;

public sealed class PlainTextDocumentExtractor : IFileFormatExtractor
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md"
    };

    public bool CanExtract(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        return Extensions.Contains(extension);
    }

    public async Task<string> ExtractAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
