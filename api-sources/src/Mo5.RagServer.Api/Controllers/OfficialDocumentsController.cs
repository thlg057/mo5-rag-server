using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Interfaces;

namespace Mo5.RagServer.Api.Controllers;

/// <summary>
/// Controller for accessing official Thomson MO5 documentation and manuals.
/// </summary>
[ApiController]
[Route("api/documents/official")]
public class OfficialDocumentsController : ControllerBase
{
    private readonly IOfficialDocumentService _officialService;
    private readonly ILogger<OfficialDocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the OfficialDocumentsController.
    /// </summary>
    /// <param name="officialService">The service for official documents.</param>
    /// <param name="logger">The logger instance.</param>
    public OfficialDocumentsController(IOfficialDocumentService officialService, ILogger<OfficialDocumentsController> logger)
    {
        _officialService = officialService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the list of all official documents with their metadata and summaries.
    /// </summary>
    /// <returns>A list of official documents.</returns>
    [HttpGet]
    public async Task<ActionResult<List<OfficialDocument>>> GetDocuments()
    {
        return Ok(await _officialService.GetOfficialDocumentsAsync());
    }

    /// <summary>
    /// Gets the metadata for a specific official document by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <returns>The document metadata or NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OfficialDocument>> GetById(Guid id)
    {
        var doc = await _officialService.GetByIdAsync(id);
        if (doc == null) return NotFound();
        return Ok(doc);
    }

    /// <summary>
    /// Downloads the physical file (PDF or Markdown) of an official document.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <returns>The file stream.</returns>
    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        try
        {
            var (content, contentType, fileName) = await _officialService.GetFileContentAsync(id);
            return File(content, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file for document {Id}", id);
            return StatusCode(500, "An internal error occurred while retrieving the file.");
        }
    }
}