using DotNetAI.Core;
using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.Prompts;
using DotNetAI.Core.Validation;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GeminiSchemaType = Google.GenAI.Types.Type;

namespace DotNetAI.Infrastructure.AI;

public sealed class GeminiService : IAIService
{
    private static readonly Schema TopicAnalysisSchema = new()
    {
        Type = GeminiSchemaType.Object,
        Required = ["summary", "technologies", "difficulty"],
        Properties = new Dictionary<string, Schema>
        {
            ["summary"] = new Schema { Type = GeminiSchemaType.String },
            ["technologies"] = new Schema
            {
                Type = GeminiSchemaType.Array,
                Items = new Schema { Type = GeminiSchemaType.String }
            },
            ["difficulty"] = new Schema
            {
                Type = GeminiSchemaType.String,
                Enum = ["Beginner", "Intermediate", "Advanced"]
            }
        }
    };

    private readonly GeminiOptions _options;
    private readonly AiOptions _aiOptions;
    private readonly IChatPromptBuilder _promptBuilder;
    private readonly ILogger<GeminiService> _logger;
    private readonly Lazy<Client> _client;

    public GeminiService(
        IOptions<GeminiOptions> options,
        IOptions<AiOptions> aiOptions,
        IChatPromptBuilder promptBuilder,
        ILogger<GeminiService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(aiOptions);
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _aiOptions = aiOptions.Value;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _client = new Lazy<Client>(CreateClient);
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ChatPrompt prompt = _promptBuilder.Build(RequireMessage(request));
        string text = await GenerateTextAsync(prompt, jsonMode: false, cancellationToken);

        return new ChatResponse
        {
            Response = text
        };
    }

    public async Task<TopicAnalysis> AnalyzeTopicAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ChatPrompt prompt = _promptBuilder.BuildTopicAnalysis(RequireMessage(request));
        string text = await GenerateTextAsync(prompt, jsonMode: true, cancellationToken);

        try
        {
            return TopicAnalysisParser.Parse(text);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Gemini structured output failed validation.");
            throw new AiProviderException(
                "The AI returned a response that could not be validated.",
                statusCode: 502);
        }
    }

    public async Task<ChatResponse> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        string text = await GenerateTextAsync(prompt, jsonMode: false, cancellationToken);
        return new ChatResponse
        {
            Response = text
        };
    }

    private async Task<string> GenerateTextAsync(ChatPrompt prompt, bool jsonMode, CancellationToken cancellationToken)
    {
        try
        {
            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = [new Part { Text = prompt.SystemInstruction }]
                },
                MaxOutputTokens = _aiOptions.MaxOutputTokens,
                Temperature = jsonMode ? 0.1f : _aiOptions.Temperature
            };

            if (jsonMode)
            {
                config.ResponseMimeType = "application/json";
                config.ResponseSchema = TopicAnalysisSchema;
            }

            var response = await _client.Value.Models.GenerateContentAsync(
                model: _options.Model,
                contents: prompt.UserMessage,
                config: config);

            string? text = response.Candidates?[0].Content?.Parts?[0].Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new AiProviderException("Gemini returned an empty response.", statusCode: 502);
            }

            _logger.LogInformation(
                "Gemini request completed. Model={Model}, JsonMode={JsonMode}",
                _options.Model,
                jsonMode);

            return text;
        }
        catch (AiProviderException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex) when (IsUnauthorized(ex))
        {
            _logger.LogError(ex, "Gemini rejected the API key.");
            throw new AiProviderException(
                "Gemini rejected the API key. Check the Gemini:ApiKey value stored in User Secrets.",
                statusCode: 401);
        }
        catch (Exception ex) when (IsQuotaExceeded(ex))
        {
            _logger.LogError(ex, "Gemini quota or rate limit was exceeded.");
            throw new AiProviderException(
                "Gemini quota or rate limit was exceeded. Wait a minute and retry, or check Google AI Studio usage.",
                statusCode: 429);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini request failed.");
            throw new AiProviderException(
                "The AI service is currently unavailable. Please try again.",
                statusCode: 502);
        }
    }

    private Client CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Gemini:ApiKey is not configured. Create a free key at https://aistudio.google.com/apikey and store it in User Secrets.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new InvalidOperationException("Gemini:Model is not configured.");
        }

        return new Client(apiKey: _options.ApiKey);
    }

    private static string RequireMessage(ChatRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(request));
        }

        return request.Message;
    }

    private static bool IsUnauthorized(Exception ex)
    {
        string message = ex.ToString();
        return message.Contains("401", StringComparison.Ordinal) ||
               message.Contains("UNAUTHENTICATED", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("API key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsQuotaExceeded(Exception ex)
    {
        string message = ex.ToString();
        return message.Contains("429", StringComparison.Ordinal) ||
               message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("quota", StringComparison.OrdinalIgnoreCase);
    }
}
