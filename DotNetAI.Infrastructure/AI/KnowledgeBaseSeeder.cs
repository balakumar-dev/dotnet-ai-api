using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetAI.Infrastructure.AI;

public sealed class KnowledgeBaseSeeder : IHostedService
{
    private readonly KnowledgeOptions _options;
    private readonly IDocumentChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<KnowledgeBaseSeeder> _logger;

    public KnowledgeBaseSeeder(
        IOptions<KnowledgeOptions> options,
        IDocumentChunker chunker,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ILogger<KnowledgeBaseSeeder> logger)
    {
        _options = options.Value;
        _chunker = chunker;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Knowledge base seeding failed. The API will still start. Wait about a minute for Gemini embedding quota to reset, then restart to index documents.");
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_options.DocumentsPath))
        {
            throw new InvalidOperationException(
                $"Knowledge documents folder was not found: {_options.DocumentsPath}");
        }

        string[] files = Directory.GetFiles(_options.DocumentsPath, "*.md", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            throw new InvalidOperationException(
                $"No markdown files were found in {_options.DocumentsPath}.");
        }

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string documentId = Path.GetFileNameWithoutExtension(file);
            string text = await File.ReadAllTextAsync(file, cancellationToken);
            IReadOnlyList<string> chunks = _chunker.Split(text, _options.ChunkSize, _options.ChunkOverlap);

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
                        FileName = fileName,
                        ChunkId = $"{documentId}-{(index + 1):00}",
                        Content = chunks[index],
                        Embedding = embedding,
                        CreatedDate = DateTimeOffset.UtcNow
                    },
                    cancellationToken);

                if (_options.EmbeddingDelayMilliseconds > 0)
                {
                    await Task.Delay(_options.EmbeddingDelayMilliseconds, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Indexed {ChunkCount} chunks from {FileName}.",
                chunks.Count,
                fileName);
        }

        _logger.LogInformation("Knowledge base ready. Chunks={ChunkCount}", _vectorStore.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
