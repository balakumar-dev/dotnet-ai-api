using DotNetAI.Core.Interfaces;

namespace DotNetAI.Core.Chunking;

public sealed class DocumentChunker : IDocumentChunker
{
    public IReadOnlyList<string> Split(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap));
        }

        string normalized = text.Replace("\r\n", "\n").Trim();
        if (normalized.Length <= chunkSize)
        {
            return [normalized];
        }

        var chunks = new List<string>();
        int start = 0;

        while (start < normalized.Length)
        {
            int length = Math.Min(chunkSize, normalized.Length - start);
            int end = start + length;

            if (end < normalized.Length)
            {
                int lastSpace = normalized.LastIndexOf(' ', end - 1, length);
                if (lastSpace > start)
                {
                    end = lastSpace;
                }
            }

            chunks.Add(normalized[start..end].Trim());
            start = Math.Max(end - overlap, start + 1);
        }

        return chunks.Where(chunk => chunk.Length > 0).ToList();
    }
}
