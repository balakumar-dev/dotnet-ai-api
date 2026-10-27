using DotNetAI.Core;
using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetAI.Infrastructure.AI;

public sealed class GeminiEmbeddingService : IEmbeddingService
{
    private const int MaxAttempts = 6;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(20);

    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiEmbeddingService> _logger;
    private readonly Lazy<Client> _client;

    public GeminiEmbeddingService(IOptions<GeminiOptions> options, ILogger<GeminiEmbeddingService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
        _client = new Lazy<Client>(CreateClient);
    }

    public async Task<float[]> EmbedAsync(
        string text,
        EmbeddingPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text to embed cannot be empty.", nameof(text));
        }

        ClientError? lastQuotaError = null;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                return await EmbedOnceAsync(text, purpose, cancellationToken);
            }
            catch (ClientError ex) when (IsQuotaExceeded(ex) && attempt < MaxAttempts)
            {
                lastQuotaError = ex;
                _logger.LogWarning(
                    "Gemini embedding quota was hit. Waiting {DelaySeconds}s, then retry {Attempt}/{MaxAttempts}.",
                    (int)RetryDelay.TotalSeconds,
                    attempt,
                    MaxAttempts);
                await Task.Delay(RetryDelay, cancellationToken);
            }
            catch (ClientError ex) when (IsQuotaExceeded(ex))
            {
                lastQuotaError = ex;
                break;
            }
        }

        _logger.LogError(lastQuotaError, "Gemini embedding quota was exceeded after retries.");
        throw new AiProviderException(
            "Gemini embedding quota was exceeded. Wait about a minute and restart the API. Chat endpoints do not use embeddings.",
            statusCode: 429);
    }

    private async Task<float[]> EmbedOnceAsync(
        string text,
        EmbeddingPurpose purpose,
        CancellationToken cancellationToken)
    {
        try
        {
            var config = new EmbedContentConfig
            {
                TaskType = purpose == EmbeddingPurpose.Query
                    ? "RETRIEVAL_QUERY"
                    : "RETRIEVAL_DOCUMENT"
            };

            EmbedContentResponse response = await _client.Value.Models.EmbedContentAsync(
                model: _options.EmbeddingModel,
                contents: text,
                config: config,
                cancellationToken: cancellationToken);

            List<double>? values = response.Embeddings?[0].Values;
            if (values is null || values.Count == 0)
            {
                throw new AiProviderException("Gemini returned an empty embedding.", statusCode: 502);
            }

            return values.Select(static value => (float)value).ToArray();
        }
        catch (AiProviderException)
        {
            throw;
        }
        catch (ClientError ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("not supported", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Gemini embedding model {Model} is not available.", _options.EmbeddingModel);
            throw new AiProviderException(
                $"Gemini embedding model '{_options.EmbeddingModel}' is not available. Set Gemini:EmbeddingModel to gemini-embedding-001.",
                statusCode: 502);
        }
        catch (ClientError)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini embedding request failed.");
            throw new AiProviderException(
                "The embedding service is currently unavailable. Please try again.",
                statusCode: 502);
        }
    }

    private Client CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Gemini:ApiKey is not configured. Store it in User Secrets.");
        }

        if (string.IsNullOrWhiteSpace(_options.EmbeddingModel))
        {
            throw new InvalidOperationException("Gemini:EmbeddingModel is not configured.");
        }

        return new Client(apiKey: _options.ApiKey);
    }

    private static bool IsQuotaExceeded(ClientError ex)
    {
        return ex.Message.Contains("Quota exceeded", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase);
    }
}
