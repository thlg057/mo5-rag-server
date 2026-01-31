using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Services;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// File system watcher service for monitoring knowledge base changes
/// </summary>
public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly ILogger<FileWatcherService> _logger;
    private FileSystemWatcher? _fileWatcher;
    private readonly object _lock = new();
    private bool _disposed = false;

    public event EventHandler<FileChangeEventArgs>? FileChanged;
    public bool IsWatching => _fileWatcher?.EnableRaisingEvents == true;

    public FileWatcherService(ILogger<FileWatcherService> logger)
    {
        _logger = logger;
    }

    public Task StartWatchingAsync(string knowledgeBasePath, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileWatcherService));

            if (IsWatching)
            {
                _logger.LogWarning("File watcher is already running");
                return Task.CompletedTask;
            }

            if (!Directory.Exists(knowledgeBasePath))
            {
                _logger.LogError("Knowledge base path does not exist: {Path}", knowledgeBasePath);
                throw new DirectoryNotFoundException($"Knowledge base path does not exist: {knowledgeBasePath}");
            }

            try
            {
                _fileWatcher = new FileSystemWatcher(knowledgeBasePath)
                {
                    Filter = "*.md",
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
                };

                // Subscribe to events
                _fileWatcher.Created += OnFileCreated;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Deleted += OnFileDeleted;
                _fileWatcher.Renamed += OnFileRenamed;
                _fileWatcher.Error += OnError;

                // Start watching
                _fileWatcher.EnableRaisingEvents = true;

                _logger.LogInformation("Started watching knowledge base directory: {Path}", knowledgeBasePath);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start file watcher for path: {Path}", knowledgeBasePath);
                throw;
            }
        }
    }

    public Task StopWatchingAsync()
    {
        lock (_lock)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Dispose();
                _fileWatcher = null;
                _logger.LogInformation("Stopped watching knowledge base directory");
            }

            return Task.CompletedTask;
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File created: {FilePath}", e.FullPath);
        FireFileChanged(e.FullPath, FileChangeType.Created);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File modified: {FilePath}", e.FullPath);
        FireFileChanged(e.FullPath, FileChangeType.Modified);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File deleted: {FilePath}", e.FullPath);
        FireFileChanged(e.FullPath, FileChangeType.Deleted);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogDebug("File renamed: {OldPath} -> {NewPath}", e.OldFullPath, e.FullPath);
        
        // Fire deleted event for old path
        FireFileChanged(e.OldFullPath!, FileChangeType.Deleted);
        
        // Fire created event for new path
        FireFileChanged(e.FullPath, FileChangeType.Renamed);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "File watcher error occurred");
    }

    private void FireFileChanged(string filePath, FileChangeType changeType)
    {
        try
        {
            var args = new FileChangeEventArgs
            {
                FilePath = filePath,
                ChangeType = changeType,
                Timestamp = DateTime.UtcNow
            };

            FileChanged?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error firing file change event for: {FilePath}", filePath);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _fileWatcher?.Dispose();
                    _disposed = true;
                }
            }
        }
    }
}
