using Microsoft.AspNetCore.Mvc;
using Mo5.RagServer.Core.Services;

namespace Mo5.RagServer.Api.Controllers;

/// <summary>
/// Controller for monitoring document ingestion
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IIngestionStatsService _statsService;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(
        IIngestionStatsService statsService,
        IFileWatcherService fileWatcherService,
        ILogger<IngestionController> logger)
    {
        _statsService = statsService;
        _fileWatcherService = fileWatcherService;
        _logger = logger;
    }

    /// <summary>
    /// Get current ingestion statistics
    /// </summary>
    /// <returns>Ingestion statistics and metrics</returns>
    [HttpGet("stats")]
    public ActionResult<IngestionStatsResponse> GetStats()
    {
        try
        {
            var stats = _statsService.GetStats();
            
            var response = new IngestionStatsResponse
            {
                TotalFilesProcessed = stats.TotalFilesProcessed,
                SuccessfulIndexings = stats.SuccessfulIndexings,
                FailedIndexings = stats.FailedIndexings,
                SuccessRate = stats.SuccessRate,
                TotalProcessingTimeMs = stats.TotalProcessingTimeMs,
                AverageProcessingTimeMs = stats.AverageProcessingTimeMs,
                TotalChunksCreated = stats.TotalChunksCreated,
                LastProcessedAt = stats.LastProcessedAt,
                LastFailureAt = stats.LastFailureAt,
                LastError = stats.LastError,
                IsWatching = _fileWatcherService.IsWatching,
                RecentActivities = stats.RecentActivities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(20)
                    .ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ingestion statistics");
            return StatusCode(500, new { error = "Failed to get ingestion statistics" });
        }
    }

    /// <summary>
    /// Get recent ingestion activities
    /// </summary>
    /// <param name="limit">Maximum number of activities to return (default: 50, max: 200)</param>
    /// <returns>List of recent activities</returns>
    [HttpGet("activities")]
    public ActionResult<List<RecentActivity>> GetRecentActivities([FromQuery] int limit = 50)
    {
        try
        {
            limit = Math.Min(Math.Max(limit, 1), 200);
            
            var stats = _statsService.GetStats();
            var activities = stats.RecentActivities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent activities");
            return StatusCode(500, new { error = "Failed to get recent activities" });
        }
    }

    /// <summary>
    /// Reset ingestion statistics
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("stats/reset")]
    public ActionResult ResetStats()
    {
        try
        {
            _statsService.ResetStats();
            _logger.LogInformation("Ingestion statistics reset");
            
            return Ok(new { message = "Ingestion statistics reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset ingestion statistics");
            return StatusCode(500, new { error = "Failed to reset statistics" });
        }
    }

    /// <summary>
    /// Get file watcher status
    /// </summary>
    /// <returns>File watcher status information</returns>
    [HttpGet("watcher/status")]
    public ActionResult<FileWatcherStatus> GetWatcherStatus()
    {
        try
        {
            var status = new FileWatcherStatus
            {
                IsWatching = _fileWatcherService.IsWatching,
                Status = _fileWatcherService.IsWatching ? "Active" : "Inactive"
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file watcher status");
            return StatusCode(500, new { error = "Failed to get watcher status" });
        }
    }
}

/// <summary>
/// Response model for ingestion statistics
/// </summary>
public class IngestionStatsResponse
{
    public int TotalFilesProcessed { get; set; }
    public int SuccessfulIndexings { get; set; }
    public int FailedIndexings { get; set; }
    public double SuccessRate { get; set; }
    public long TotalProcessingTimeMs { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int TotalChunksCreated { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public string? LastError { get; set; }
    public bool IsWatching { get; set; }
    public List<RecentActivity> RecentActivities { get; set; } = new();
}

/// <summary>
/// File watcher status information
/// </summary>
public class FileWatcherStatus
{
    public bool IsWatching { get; set; }
    public string Status { get; set; } = string.Empty;
}
