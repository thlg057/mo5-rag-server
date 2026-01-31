using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mo5.RagServer.Core.Interfaces;

namespace Mo5.RagServer.Api.Controllers;

/// <summary>
/// Controller for document indexing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IndexController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexController> _logger;

    /// <summary>
    /// Constructs the index controller with required services.
    /// </summary>
    public IndexController(
        IDocumentService documentService,
        IConfiguration configuration,
        ILogger<IndexController> logger)
    {
        _documentService = documentService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Index all documents in the knowledge base
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Number of documents processed</returns>
    [HttpPost("all")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<IndexResult>> IndexAllDocuments(CancellationToken cancellationToken)
    {
        try
        {
            var knowledgeBasePath = _configuration.GetValue<string>("RagSettings:KnowledgeBasePath", "./knowledge") ?? "./knowledge";
            var fullPath = Path.GetFullPath(knowledgeBasePath);
            
            _logger.LogInformation("Starting indexing of all documents in: {Path}", fullPath);
            
            var startTime = DateTime.UtcNow;
            var processedCount = await _documentService.IndexDocumentsAsync(fullPath, cancellationToken);
            var duration = DateTime.UtcNow - startTime;

            var result = new IndexResult
            {
                ProcessedDocuments = processedCount,
                KnowledgeBasePath = fullPath,
                Duration = duration,
                Success = true,
                Message = $"Successfully indexed {processedCount} documents"
            };

            _logger.LogInformation("Indexing completed: {ProcessedCount} documents in {Duration}ms", 
                processedCount, duration.TotalMilliseconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index documents");
            
            return StatusCode(500, new IndexResult
            {
                Success = false,
                Message = $"Indexing failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Index a specific document
    /// </summary>
    /// <param name="request">Request containing the relative file path to index.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Indexing result</returns>
    [HttpPost("document")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<IndexResult>> IndexDocument(
        [FromBody] IndexDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest(new IndexResult
                {
                    Success = false,
                    Message = "FilePath is required"
                });
            }

            var knowledgeBasePath = _configuration.GetValue<string>("RagSettings:KnowledgeBasePath", "./knowledge") ?? "./knowledge";
            var fullPath = Path.Combine(Path.GetFullPath(knowledgeBasePath), request.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new IndexResult
                {
                    Success = false,
                    Message = $"File not found: {request.FilePath}"
                });
            }

            _logger.LogInformation("Indexing document: {FilePath}", fullPath);
            
            var startTime = DateTime.UtcNow;
            var document = await _documentService.IndexDocumentAsync(fullPath, cancellationToken);
            var duration = DateTime.UtcNow - startTime;

            var result = new IndexResult
            {
                ProcessedDocuments = 1,
                KnowledgeBasePath = knowledgeBasePath,
                Duration = duration,
                Success = true,
                Message = $"Successfully indexed document: {document.Title}",
                DocumentId = document.Id
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document: {FilePath}", request.FilePath);
            
            return StatusCode(500, new IndexResult
            {
                Success = false,
                Message = $"Indexing failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get indexing status and statistics
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<IndexStatus>> GetIndexStatus(CancellationToken cancellationToken)
    {
        try
        {
            var documents = await _documentService.GetDocumentsAsync(cancellationToken: cancellationToken);
            var totalChunks = documents.Sum(d => d.Chunks.Count);
            var lastIndexed = documents.Any() ? documents.Max(d => d.UpdatedAt) : (DateTime?)null;

            var status = new IndexStatus
            {
                TotalDocuments = documents.Count,
                TotalChunks = totalChunks,
                LastIndexed = lastIndexed,
                KnowledgeBasePath = _configuration.GetValue<string>("RagSettings:KnowledgeBasePath", "./knowledge") ?? "./knowledge",
                DocumentsByTag = documents
                    .SelectMany(d => d.DocumentTags.Where(dt => dt.Tag.IsActive))
                    .GroupBy(dt => dt.Tag.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get index status");
            return StatusCode(500, "Failed to get index status");
        }
    }
}

/// <summary>
/// Request model for indexing a specific document
/// </summary>
public class IndexDocumentRequest
{
    /// <summary>
    /// Relative path to the document from knowledge base root
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Result of an indexing operation
/// </summary>
public class IndexResult
{
    /// <summary>Whether the indexing operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Human-friendly message describing the result.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Number of processed documents.</summary>
    public int ProcessedDocuments { get; set; }

    /// <summary>Knowledge base path used for indexing.</summary>
    public string KnowledgeBasePath { get; set; } = string.Empty;

    /// <summary>Duration of the indexing operation.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Optional document id when a single document was indexed.</summary>
    public Guid? DocumentId { get; set; }
}

/// <summary>
/// Current indexing status
/// </summary>
public class IndexStatus
{
    /// <summary>Total number of indexed documents.</summary>
    public int TotalDocuments { get; set; }

    /// <summary>Total number of chunks across all documents.</summary>
    public int TotalChunks { get; set; }

    /// <summary>Timestamp of the last indexing operation, if any.</summary>
    public DateTime? LastIndexed { get; set; }

    /// <summary>Knowledge base path used by the service.</summary>
    public string KnowledgeBasePath { get; set; } = string.Empty;

    /// <summary>Counts of documents grouped by tag name.</summary>
    public Dictionary<string, int> DocumentsByTag { get; set; } = new();
}
