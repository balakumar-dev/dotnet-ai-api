using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetAI.Infrastructure.AI;

public sealed class RagChatService : IRagChatService
{
    private readonly IKnowledgeSearchService _searchService;
    private readonly IAIService _aiService;
    private readonly IChatPromptBuilder _promptBuilder;
    private readonly KnowledgeOptions _options;
    private readonly ILogger<RagChatService> _logger;

    public RagChatService(
        IKnowledgeSearchService searchService,
        IAIService aiService,
        IChatPromptBuilder promptBuilder,
        IOptions<KnowledgeOptions> options,
        ILogger<RagChatService> logger)
    {
        _searchService = searchService;
        _aiService = aiService;
        _promptBuilder = promptBuilder;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(request));
        }

        IReadOnlyList<KnowledgeSearchMatch> matches;
        try
        {
            KnowledgeSearchResponse search = await _searchService.SearchAsync(
                new KnowledgeSearchRequest
                {
                    Query = request.Message,
                    TopK = _options.RagTopK
                },
                cancellationToken);

            matches = search.Results;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Knowledge base is empty", StringComparison.Ordinal))
        {
            _logger.LogWarning("RAG chat skipped generation because the knowledge base is empty.");
            return new ChatResponse { Response = PromptTemplates.InsufficientContext };
        }

        IReadOnlyList<KnowledgeSearchMatch> relevant = matches
            .Where(match => match.Score >= _options.MinimumSimilarityScore)
            .ToList();

        if (relevant.Count == 0)
        {
            _logger.LogInformation(
                "RAG found no chunks above the similarity threshold {Threshold}.",
                _options.MinimumSimilarityScore);

            return new ChatResponse { Response = PromptTemplates.InsufficientContext };
        }

        ChatPrompt prompt = _promptBuilder.BuildRag(request.Message, relevant);
        return await _aiService.CompleteAsync(prompt, cancellationToken);
    }
}
