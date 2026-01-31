using Mo5.RagServer.Core.Services;
using System.Collections.Concurrent;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// In-memory implementation of ingestion statistics service
/// </summary>
public class IngestionStatsService : IIngestionStatsService
{
    private readonly object _lock = new();
    private readonly ConcurrentQueue<RecentActivity> _recentActivities = new();
    private const int MaxRecentActivities = 100;

    private int _totalFilesProcessed;
    private int _successfulIndexings;
    private int _failedIndexings;
    private long _totalProcessingTimeMs;
    private int _totalChunksCreated;
    private DateTime? _lastProcessedAt;
    private DateTime? _lastFailureAt;
    private string? _lastError;

    public void RecordIndexingSuccess(string filePath, long processingTimeMs, int chunkCount)
    {
        lock (_lock)
        {
            _totalFilesProcessed++;
            _successfulIndexings++;
            _totalProcessingTimeMs += processingTimeMs;
            _totalChunksCreated += chunkCount;
            _lastProcessedAt = DateTime.UtcNow;

            AddRecentActivity(new RecentActivity
            {
                Timestamp = DateTime.UtcNow,
                FilePath = filePath,
                Action = "Indexed",
                Success = true,
                ProcessingTimeMs = processingTimeMs
            });
        }
    }

    public void RecordIndexingFailure(string filePath, string error)
    {
        lock (_lock)
        {
            _totalFilesProcessed++;
            _failedIndexings++;
            _lastFailureAt = DateTime.UtcNow;
            _lastError = error;

            AddRecentActivity(new RecentActivity
            {
                Timestamp = DateTime.UtcNow,
                FilePath = filePath,
                Action = "Failed",
                Success = false,
                ProcessingTimeMs = 0,
                Error = error
            });
        }
    }

    public IngestionStats GetStats()
    {
        lock (_lock)
        {
            return new IngestionStats
            {
                TotalFilesProcessed = _totalFilesProcessed,
                SuccessfulIndexings = _successfulIndexings,
                FailedIndexings = _failedIndexings,
                TotalProcessingTimeMs = _totalProcessingTimeMs,
                TotalChunksCreated = _totalChunksCreated,
                LastProcessedAt = _lastProcessedAt,
                LastFailureAt = _lastFailureAt,
                LastError = _lastError,
                RecentActivities = _recentActivities.ToList()
            };
        }
    }

    public void ResetStats()
    {
        lock (_lock)
        {
            _totalFilesProcessed = 0;
            _successfulIndexings = 0;
            _failedIndexings = 0;
            _totalProcessingTimeMs = 0;
            _totalChunksCreated = 0;
            _lastProcessedAt = null;
            _lastFailureAt = null;
            _lastError = null;
            
            _recentActivities.Clear();
        }
    }

    private void AddRecentActivity(RecentActivity activity)
    {
        _recentActivities.Enqueue(activity);

        // Keep only the most recent activities
        while (_recentActivities.Count > MaxRecentActivities)
        {
            _recentActivities.TryDequeue(out _);
        }
    }
}
