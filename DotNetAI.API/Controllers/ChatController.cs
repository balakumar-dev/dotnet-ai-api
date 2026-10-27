using DotNetAI.Core;
using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAI.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IRagChatService _ragChatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IAIService aiService,
        IRagChatService ragChatService,
        ILogger<ChatController> logger)
    {
        _aiService = aiService;
        _ragChatService = ragChatService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        try
        {
            ChatResponse response = await _aiService.ChatAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AiProviderException ex)
        {
            _logger.LogError(ex, "Chat request failed due to an AI provider error.");
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Chat request failed due to a configuration error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("knowledge")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> KnowledgeChat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        try
        {
            ChatResponse response = await _ragChatService.AskAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AiProviderException ex)
        {
            _logger.LogError(ex, "RAG chat request failed due to an AI provider error.");
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "RAG chat request failed due to a configuration error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(TopicAnalysis), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TopicAnalysis>> Analyze(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        try
        {
            TopicAnalysis analysis = await _aiService.AnalyzeTopicAsync(request, cancellationToken);
            return Ok(analysis);
        }
        catch (AiProviderException ex)
        {
            _logger.LogError(ex, "Topic analysis failed due to an AI provider error.");
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Topic analysis failed due to a configuration error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
