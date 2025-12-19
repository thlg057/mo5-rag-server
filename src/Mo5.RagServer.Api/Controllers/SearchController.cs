using Microsoft.AspNetCore.Mvc;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Models;

namespace Mo5.RagServer.Api.Controllers;

/// <summary>
/// Controller for semantic search operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IDocumentService documentService,
        ILogger<SearchController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Perform semantic search across the knowledge base
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Search results ordered by relevance</returns>
    [HttpPost]
    public async Task<ActionResult<SearchResponse>> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query is required" });
            }

            // Validate and sanitize request parameters
            request.MaxResults = Math.Min(Math.Max(request.MaxResults, 1), 50);
            request.MinSimilarityScore = Math.Max(Math.Min(request.MinSimilarityScore, 1.0f), 0.0f);

            _logger.LogInformation("Performing semantic search for query: {Query}", request.Query);

            var response = await _documentService.SearchAsync(request, cancellationToken);

            _logger.LogInformation("Search completed: {ResultCount} results in {ExecutionTime}ms", 
                response.TotalResults, response.ExecutionTimeMs);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform search for query: {Query}", request.Query);
            return StatusCode(500, new { error = "Search failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Perform a simple search with query string
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="maxResults">Maximum number of results (default: 10, max: 50)</param>
    /// <param name="minScore">Minimum similarity score (default: 0.7)</param>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <param name="includeMetadata">Include document metadata in response</param>
    /// <param name="includeContext">Include surrounding context for each result</param>
    /// <returns>Search results</returns>
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> SearchGet(
        [FromQuery] string q,
        [FromQuery] int maxResults = 10,
        [FromQuery] float minScore = 0.7f,
        [FromQuery] string? tags = null,
        [FromQuery] bool includeMetadata = true,
        [FromQuery] bool includeContext = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Query parameter 'q' is required" });
            }

            var tagList = string.IsNullOrWhiteSpace(tags) 
                ? new List<string>() 
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .ToList();

            var request = new SearchRequest
            {
                Query = q,
                MaxResults = Math.Min(Math.Max(maxResults, 1), 50),
                MinSimilarityScore = Math.Max(Math.Min(minScore, 1.0f), 0.0f),
                Tags = tagList,
                IncludeMetadata = includeMetadata,
                IncludeContext = includeContext
            };

            return await Search(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform GET search for query: {Query}", q);
            return StatusCode(500, new { error = "Search failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Get search suggestions based on partial query
    /// </summary>
    /// <param name="partial">Partial query text</param>
    /// <param name="limit">Maximum number of suggestions</param>
    /// <returns>List of suggested search terms</returns>
    [HttpGet("suggestions")]
    public Task<ActionResult<List<string>>> GetSuggestions(
        [FromQuery] string partial,
        [FromQuery] int limit = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partial) || partial.Length < 2)
            {
                return Task.FromResult<ActionResult<List<string>>>(Ok(new List<string>()));
            }

            // Simple implementation: return common MO5-related terms
            // In a more advanced implementation, this could analyze existing content
            var suggestions = new List<string>
            {
                "6809 assembly programming",
                "C programming examples",
                "graphics mode programming",
                "text mode display",
                "memory map",
                "hardware registers",
                "compilation tools",
                "BASIC programming",
                "interrupt handling",
                "disk image creation"
            };

            var filteredSuggestions = suggestions
                .Where(s => s.Contains(partial, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();

            return Task.FromResult<ActionResult<List<string>>>(Ok(filteredSuggestions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get search suggestions for: {Partial}", partial);
            return Task.FromResult<ActionResult<List<string>>>(StatusCode(500, new { error = "Failed to get suggestions" }));
        }
    }
}
