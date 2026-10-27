namespace DotNetAI.Core.Interfaces;

public interface IDocumentChunker
{
    IReadOnlyList<string> Split(string text, int chunkSize, int overlap);
}
