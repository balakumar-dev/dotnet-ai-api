using System.ClientModel;
using DotNetAI.Core;
using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.Prompts;
using DotNetAI.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace DotNetAI.Infrastructure.AI;

public sealed class OpenAIService : IAIService
{
    private readonly OpenAIOptions _options;
    private readonly AiOptions _aiOptions;
    private readonly IChatPromptBuilder _promptBuilder;
    private readonly ILogger<OpenAIService> _logger;
    private readonly Lazy<ChatClient> _chatClient;

    public OpenAIService(
        IOptions<OpenAIOptions> options,
        IOptions<AiOptions> aiOptions,
        IChatPromptBuilder promptBuilder,
        ILogger<OpenAIService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(aiOptions);
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _aiOptions = aiOptions.Value;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _chatClient = new Lazy<ChatClient>(CreateChatClient);
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
            _logger.LogWarning(ex, "OpenAI structured output failed validation.");
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
            var completionOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _aiOptions.MaxOutputTokens,
                Temperature = jsonMode ? 0.1f : _aiOptions.Temperature
            };

            if (jsonMode)
            {
                completionOptions.ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat();
            }

            ChatCompletion completion = await _chatClient.Value.CompleteChatAsync(
                [
                    new SystemChatMessage(prompt.SystemInstruction),
                    new UserChatMessage(prompt.UserMessage)
                ],
                completionOptions,
                cancellationToken);

            string text = completion.Content[0].Text;

            _logger.LogInformation(
                "OpenAI request completed. Model={Model}, JsonMode={JsonMode}",
                completion.Model,
                jsonMode);

            return text;
        }
        catch (ClientResultException ex) when (ex.Status == 401)
        {
            _logger.LogError(ex, "OpenAI rejected the API key.");
            throw new AiProviderException(
                "OpenAI rejected the API key. Check the value stored in User Secrets.",
                statusCode: 401);
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogError(ex, "OpenAI quota or rate limit was exceeded.");
            throw new AiProviderException(
                "OpenAI has no remaining credits (HTTP 429 insufficient_quota). Add billing credits and retry.",
                statusCode: 429);
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "OpenAI request failed with status {Status}.", ex.Status);
            throw new AiProviderException(
                "The AI service is currently unavailable. Please try again.",
                statusCode: 502);
        }
    }

    private ChatClient CreateChatClient()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey is not configured. For local development, set it with User Secrets.");
        }

        if (_options.ApiKey.StartsWith("crsr_", StringComparison.OrdinalIgnoreCase) ||
            _options.ApiKey.StartsWith("key_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The configured key looks like a Cursor API key. This app calls api.openai.com and needs an OpenAI Platform key (usually starting with sk-), plus OpenAI billing credits. A Cursor subscription does not pay for this endpoint.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new InvalidOperationException("OpenAI:Model is not configured.");
        }

        return new ChatClient(model: _options.Model, apiKey: _options.ApiKey);
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
}
