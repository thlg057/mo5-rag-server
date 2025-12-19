namespace Mo5.RagServer.Core.Services;

/// <summary>
/// Service for tracking ingestion statistics and metrics
/// </summary>
public interface IIngestionStatsService
{
    /// <summary>
    /// Record a successful document indexing
    /// </summary>
    /// <param name="filePath">Path of the indexed file</param>
    /// <param name="processingTimeMs">Time taken to process in milliseconds</param>
    /// <param name="chunkCount">Number of chunks created</param>
    void RecordIndexingSuccess(string filePath, long processingTimeMs, int chunkCount);
    
    /// <summary>
    /// Record a failed document indexing
    /// </summary>
    /// <param name="filePath">Path of the file that failed</param>
    /// <param name="error">Error message</param>
    void RecordIndexingFailure(string filePath, string error);
    
    /// <summary>
    /// Get current ingestion statistics
    /// </summary>
    /// <returns>Current statistics</returns>
    IngestionStats GetStats();
    
    /// <summary>
    /// Reset all statistics
    /// </summary>
    void ResetStats();
}

/// <summary>
/// Ingestion statistics
/// </summary>
public class IngestionStats
{
    public int TotalFilesProcessed { get; set; }
    public int SuccessfulIndexings { get; set; }
    public int FailedIndexings { get; set; }
    public long TotalProcessingTimeMs { get; set; }
    public int TotalChunksCreated { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public DateTime? LastFailureAt { get; set; }
    public string? LastError { get; set; }
    public List<RecentActivity> RecentActivities { get; set; } = new();
    
    public double SuccessRate => TotalFilesProcessed > 0 ? (double)SuccessfulIndexings / TotalFilesProcessed * 100 : 0;
    public double AverageProcessingTimeMs => SuccessfulIndexings > 0 ? (double)TotalProcessingTimeMs / SuccessfulIndexings : 0;
}

/// <summary>
/// Recent ingestion activity
/// </summary>
public class RecentActivity
{
    public DateTime Timestamp { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string? Error { get; set; }
}
