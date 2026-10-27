namespace DotNetAI.Core.Interfaces;

public interface IFileFormatExtractor
{
    bool CanExtract(string fileName);

    Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
