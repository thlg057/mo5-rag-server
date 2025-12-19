using Microsoft.AspNetCore.Mvc;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Interfaces;

namespace Mo5.RagServer.Api.Controllers;

/// <summary>
/// Controller for document management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents with optional tag filtering
    /// </summary>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <returns>List of documents</returns>
    [HttpGet]
    public async Task<ActionResult<List<DocumentSummary>>> GetDocuments(
        [FromQuery] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tagList = string.IsNullOrWhiteSpace(tags) 
                ? null 
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .ToList();

            var documents = await _documentService.GetDocumentsAsync(tagList, cancellationToken);

            var summaries = documents.Select(d => new DocumentSummary
            {
                Id = d.Id,
                FileName = d.FileName,
                Title = d.Title,
                FilePath = d.FilePath,
                FileSize = d.FileSize,
                LastModified = d.LastModified,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                ChunkCount = d.Chunks.Count,
                Tags = d.DocumentTags
                    .Where(dt => dt.Tag.IsActive)
                    .Select(dt => new TagSummary
                    {
                        Name = dt.Tag.Name,
                        Category = dt.Tag.Category,
                        Color = dt.Tag.Color,
                        Confidence = dt.Confidence
                    })
                    .ToList()
            }).ToList();

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get documents");
            return StatusCode(500, new { error = "Failed to get documents" });
        }
    }

    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDetail>> GetDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetDocumentAsync(id, cancellationToken);

            if (document == null)
            {
                return NotFound(new { error = "Document not found" });
            }

            var detail = new DocumentDetail
            {
                Id = document.Id,
                FileName = document.FileName,
                Title = document.Title,
                FilePath = document.FilePath,
                Content = document.Content,
                FileSize = document.FileSize,
                ContentHash = document.ContentHash,
                LastModified = document.LastModified,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                ChunkCount = document.Chunks.Count,
                Tags = document.DocumentTags
                    .Where(dt => dt.Tag.IsActive)
                    .Select(dt => new TagSummary
                    {
                        Name = dt.Tag.Name,
                        Category = dt.Tag.Category,
                        Color = dt.Tag.Color,
                        Confidence = dt.Confidence
                    })
                    .ToList(),
                Chunks = document.Chunks
                    .OrderBy(c => c.ChunkIndex)
                    .Select(c => new ChunkSummary
                    {
                        Id = c.Id,
                        ChunkIndex = c.ChunkIndex,
                        Content = c.Content.Length > 200 ? c.Content.Substring(0, 200) + "..." : c.Content,
                        StartPosition = c.StartPosition,
                        EndPosition = c.EndPosition,
                        TokenCount = c.TokenCount,
                        SectionHeading = c.SectionHeading
                    })
                    .ToList()
            };

            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document: {DocumentId}", id);
            return StatusCode(500, new { error = "Failed to get document" });
        }
    }

    /// <summary>
    /// Delete a document (soft delete)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _documentService.DeleteDocumentAsync(id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = "Document not found" });
            }

            _logger.LogInformation("Document deleted: {DocumentId}", id);
            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document: {DocumentId}", id);
            return StatusCode(500, new { error = "Failed to delete document" });
        }
    }

    /// <summary>
    /// Get all available tags
    /// </summary>
    /// <returns>List of tags</returns>
    [HttpGet("tags")]
    public async Task<ActionResult<List<TagDetail>>> GetTags(CancellationToken cancellationToken)
    {
        try
        {
            var tags = await _documentService.GetTagsAsync(cancellationToken);

            var tagDetails = tags.Select(t => new TagDetail
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                Color = t.Color,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(tagDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tags");
            return StatusCode(500, new { error = "Failed to get tags" });
        }
    }
}

/// <summary>
/// Document summary for list views
/// </summary>
public class DocumentSummary
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ChunkCount { get; set; }
    public List<TagSummary> Tags { get; set; } = new();
}

/// <summary>
/// Detailed document information
/// </summary>
public class DocumentDetail : DocumentSummary
{
    public string Content { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public List<ChunkSummary> Chunks { get; set; } = new();
}

/// <summary>
/// Tag summary information
/// </summary>
public class TagSummary
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public float Confidence { get; set; }
}

/// <summary>
/// Detailed tag information
/// </summary>
public class TagDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Chunk summary information
/// </summary>
public class ChunkSummary
{
    public Guid Id { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int TokenCount { get; set; }
    public string? SectionHeading { get; set; }
}
