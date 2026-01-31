namespace Mo5.RagServer.Core.Services;

/// <summary>
/// Service for monitoring file system changes in the knowledge base
/// </summary>
public interface IFileWatcherService
{
    /// <summary>
    /// Start monitoring the knowledge base directory for changes
    /// </summary>
    /// <param name="knowledgeBasePath">Path to monitor</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartWatchingAsync(string knowledgeBasePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stop monitoring the knowledge base directory
    /// </summary>
    Task StopWatchingAsync();
    
    /// <summary>
    /// Event fired when a file is created, modified, or deleted
    /// </summary>
    event EventHandler<FileChangeEventArgs>? FileChanged;
    
    /// <summary>
    /// Check if the service is currently watching
    /// </summary>
    bool IsWatching { get; }
}

/// <summary>
/// Event arguments for file change notifications
/// </summary>
public class FileChangeEventArgs : EventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public FileChangeType ChangeType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of file changes
/// </summary>
public enum FileChangeType
{
    Created,
    Modified,
    Deleted,
    Renamed
}
