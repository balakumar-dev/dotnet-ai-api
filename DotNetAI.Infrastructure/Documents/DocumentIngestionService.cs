using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.TextProcessing;
using DotNetAI.Infrastructure.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetAI.Infrastructure.Documents;

public sealed class DocumentIngestionService : IDocumentIngestionService
{
    private readonly KnowledgeOptions _options;
    private readonly IDocumentTextExtractor _textExtractor;
    private readonly IDocumentChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(
        IOptions<KnowledgeOptions> options,
        IDocumentTextExtractor textExtractor,
        IDocumentChunker chunker,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ILogger<DocumentIngestionService> logger)
    {
        _options = options.Value;
        _textExtractor = textExtractor;
        _chunker = chunker;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<DocumentIngestionResult> IngestAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length == 0)
        {
            throw new InvalidOperationException("The uploaded file is empty.");
        }

        if (buffer.Length > _options.MaxUploadBytes)
        {
            throw new ArgumentException($"File is too large. Maximum size is {_options.MaxUploadBytes} bytes.");
        }

        buffer.Position = 0;
        string extracted = await _textExtractor.ExtractAsync(buffer, fileName, cancellationToken);
        string cleaned = TextCleaner.Clean(extracted);

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            throw new InvalidOperationException("No text could be extracted from the document.");
        }

        string documentId = Path.GetFileNameWithoutExtension(fileName);
        string storedFileName = Path.GetFileName(fileName);
        DateTimeOffset createdDate = DateTimeOffset.UtcNow;

        await _vectorStore.DeleteByDocumentIdAsync(documentId, cancellationToken);

        IReadOnlyList<string> chunks = _chunker.Split(cleaned, _options.ChunkSize, _options.ChunkOverlap);

        for (int index = 0; index < chunks.Count; index++)
        {
            float[] embedding = await _embeddingService.EmbedAsync(
                chunks[index],
                EmbeddingPurpose.Document,
                cancellationToken);

            await _vectorStore.UpsertAsync(
                new KnowledgeChunk
                {
                    DocumentId = documentId,
                    FileName = storedFileName,
                    ChunkId = $"{documentId}-{(index + 1):00}",
                    Content = chunks[index],
                    Embedding = embedding,
                    CreatedDate = createdDate
                },
                cancellationToken);

            if (_options.EmbeddingDelayMilliseconds > 0 && index < chunks.Count - 1)
            {
                await Task.Delay(_options.EmbeddingDelayMilliseconds, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Ingested {FileName} as {ChunkCount} chunks.",
            storedFileName,
            chunks.Count);

        return new DocumentIngestionResult
        {
            DocumentId = documentId,
            FileName = storedFileName,
            ChunkCount = chunks.Count,
            CreatedDate = createdDate
        };
    }
}
