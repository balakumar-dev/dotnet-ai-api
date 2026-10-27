using DotNetAI.Core;
using DotNetAI.Core.DTOs;
using DotNetAI.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAI.API.Controllers;

[ApiController]
[Route("api/knowledge")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeSearchService _searchService;
    private readonly IDocumentIngestionService _ingestionService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<KnowledgeController> _logger;

    public KnowledgeController(
        IKnowledgeSearchService searchService,
        IDocumentIngestionService ingestionService,
        IVectorStore vectorStore,
        ILogger<KnowledgeController> logger)
    {
        _searchService = searchService;
        _ingestionService = ingestionService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    [HttpGet("documents")]
    [ProducesResponseType(typeof(IReadOnlyList<KnowledgeDocumentInfo>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<KnowledgeDocumentInfo>> ListDocuments()
    {
        return Ok(_vectorStore.GetDocuments());
    }

    [HttpPost("documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentIngestionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentIngestionResult>> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "A non-empty file is required." });
        }

        try
        {
            await using Stream stream = file.OpenReadStream();
            DocumentIngestionResult result = await _ingestionService.IngestAsync(
                file.FileName,
                stream,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (AiProviderException ex)
        {
            _logger.LogError(ex, "Document ingestion failed due to an embedding provider error.");
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Document ingestion failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(KnowledgeSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeSearchResponse>> Search(
        [FromBody] KnowledgeSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new { error = "Query is required." });
        }

        try
        {
            KnowledgeSearchResponse response = await _searchService.SearchAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (AiProviderException ex)
        {
            _logger.LogError(ex, "Knowledge search failed due to an embedding provider error.");
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Knowledge search failed due to a configuration error.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}
