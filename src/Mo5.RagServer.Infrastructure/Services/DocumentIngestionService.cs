using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Services;
using Mo5.RagServer.Infrastructure.Data;
using System.Collections.Concurrent;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Background service for automatic document ingestion
/// </summary>
public class DocumentIngestionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger<DocumentIngestionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentQueue<FileChangeEventArgs> _processingQueue = new();
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly Timer _batchProcessingTimer;
    private readonly TimeSpan _batchDelay = TimeSpan.FromSeconds(5); // Wait 5 seconds before processing changes

    public DocumentIngestionService(
        IServiceProvider serviceProvider,
        IFileWatcherService fileWatcherService,
        ILogger<DocumentIngestionService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _fileWatcherService = fileWatcherService;
        _logger = logger;
        _configuration = configuration;

        // Subscribe to file change events
        _fileWatcherService.FileChanged += OnFileChanged;

        // Timer for batch processing (to avoid processing rapid successive changes)
        _batchProcessingTimer = new Timer(ProcessQueuedChanges, null, Timeout.Infinite, Timeout.Infinite);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document ingestion service starting...");

        try
        {
            // Perform initial indexing
            await PerformInitialIndexing(stoppingToken);

            // Start file watching
            var knowledgeBasePath = GetKnowledgeBasePath();
            await _fileWatcherService.StartWatchingAsync(knowledgeBasePath, stoppingToken);

            _logger.LogInformation("Document ingestion service started successfully");

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Document ingestion service is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document ingestion service encountered an error");
            throw;
        }
        finally
        {
            await _fileWatcherService.StopWatchingAsync();
            _logger.LogInformation("Document ingestion service stopped");
        }
    }

    private async Task PerformInitialIndexing(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Performing initial document indexing...");

            using var scope = _serviceProvider.CreateScope();
            var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();

            var knowledgeBasePath = GetKnowledgeBasePath();
            var processedCount = await documentService.IndexDocumentsAsync(knowledgeBasePath, cancellationToken);

            _logger.LogInformation("Initial indexing completed: {ProcessedCount} documents", processedCount);

            // Initialize TF-IDF vocabulary with existing documents
            await InitializeTfIdfVocabulary(scope, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform initial indexing");
            // Don't throw - continue with file watching even if initial indexing fails
        }
    }

    private async Task InitializeTfIdfVocabulary(IServiceScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

            // Check if it's SimpleTfIdfEmbeddingService
            if (embeddingService is SimpleTfIdfEmbeddingService tfIdfService)
            {
                _logger.LogInformation("Initializing TF-IDF vocabulary with existing chunks...");

                var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();

                // Get all chunks from active documents
                var chunks = await dbContext.DocumentChunks
                    .Where(c => c.Document.IsActive)
                    .ToListAsync(cancellationToken);

                if (chunks.Any())
                {
                    var chunkContents = chunks.Select(c => c.Content).ToList();
                    await tfIdfService.InitializeWithCorpusAsync(chunkContents);

                    _logger.LogInformation("TF-IDF vocabulary initialized with {ChunkCount} chunks", chunks.Count);

                    // Regenerate embeddings for all chunks with the new vocabulary
                    _logger.LogInformation("Regenerating embeddings for all chunks with new vocabulary...");

                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];
                        var newEmbedding = await tfIdfService.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
                        chunk.Embedding = newEmbedding;

                        if ((i + 1) % 50 == 0)
                        {
                            _logger.LogInformation("Regenerated {Count}/{Total} embeddings", i + 1, chunks.Count);
                        }
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Successfully regenerated all {ChunkCount} embeddings", chunks.Count);
                }
                else
                {
                    _logger.LogWarning("No chunks found to initialize TF-IDF vocabulary");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize TF-IDF vocabulary");
            // Don't throw - this is not critical
        }
    }

    private void OnFileChanged(object? sender, FileChangeEventArgs e)
    {
        _logger.LogDebug("File change detected: {FilePath} ({ChangeType})", e.FilePath, e.ChangeType);
        
        // Add to processing queue
        _processingQueue.Enqueue(e);
        
        // Reset the timer to batch process changes
        _batchProcessingTimer.Change(_batchDelay, Timeout.InfiniteTimeSpan);
    }

    private async void ProcessQueuedChanges(object? state)
    {
        if (!await _processingSemaphore.WaitAsync(100))
        {
            // If we can't acquire the semaphore quickly, reschedule
            _batchProcessingTimer.Change(_batchDelay, Timeout.InfiniteTimeSpan);
            return;
        }

        try
        {
            var changesToProcess = new List<FileChangeEventArgs>();
            
            // Dequeue all pending changes
            while (_processingQueue.TryDequeue(out var change))
            {
                changesToProcess.Add(change);
            }

            if (!changesToProcess.Any())
                return;

            _logger.LogInformation("Processing {Count} file changes", changesToProcess.Count);

            // Group changes by file path and keep only the latest change per file
            var latestChanges = changesToProcess
                .GroupBy(c => c.FilePath)
                .Select(g => g.OrderByDescending(c => c.Timestamp).First())
                .ToList();

            using var scope = _serviceProvider.CreateScope();
            var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();

            foreach (var change in latestChanges)
            {
                await ProcessFileChange(documentService, change);
            }

            _logger.LogInformation("Completed processing file changes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queued file changes");
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    private async Task ProcessFileChange(IDocumentService documentService, FileChangeEventArgs change)
    {
        try
        {
            switch (change.ChangeType)
            {
                case FileChangeType.Created:
                case FileChangeType.Modified:
                case FileChangeType.Renamed:
                    if (File.Exists(change.FilePath))
                    {
                        _logger.LogInformation("Indexing file: {FilePath}", change.FilePath);
                        await documentService.IndexDocumentAsync(change.FilePath);
                        _logger.LogDebug("Successfully indexed: {FilePath}", change.FilePath);
                    }
                    else
                    {
                        _logger.LogWarning("File no longer exists, skipping: {FilePath}", change.FilePath);
                    }
                    break;

                case FileChangeType.Deleted:
                    _logger.LogInformation("File deleted, marking as inactive: {FilePath}", change.FilePath);
                    await HandleFileDeleted(documentService, change.FilePath);
                    break;

                default:
                    _logger.LogWarning("Unknown file change type: {ChangeType}", change.ChangeType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process file change: {FilePath} ({ChangeType})", 
                change.FilePath, change.ChangeType);
        }
    }

    private async Task HandleFileDeleted(IDocumentService documentService, string filePath)
    {
        try
        {
            // Find document by file path and mark as inactive
            var documents = await documentService.GetDocumentsAsync();
            var relativePath = GetRelativePath(filePath);
            
            var document = documents.FirstOrDefault(d => 
                d.FilePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

            if (document != null)
            {
                await documentService.DeleteDocumentAsync(document.Id);
                _logger.LogInformation("Marked document as inactive: {DocumentTitle}", document.Title);
            }
            else
            {
                _logger.LogWarning("No document found for deleted file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle file deletion: {FilePath}", filePath);
        }
    }

    private string GetKnowledgeBasePath()
    {
        var path = _configuration.GetValue<string>("RagSettings:KnowledgeBasePath", "./knowledge") ?? "./knowledge";
        return Path.GetFullPath(path);
    }

    private string GetRelativePath(string fullPath)
    {
        var knowledgeBasePath = GetKnowledgeBasePath();
        var fullFilePath = Path.GetFullPath(fullPath);

        if (fullFilePath.StartsWith(knowledgeBasePath, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetRelativePath(knowledgeBasePath, fullFilePath).Replace('\\', '/');
        }

        return Path.GetFileName(fullPath);
    }

    public override void Dispose()
    {
        _batchProcessingTimer?.Dispose();
        _processingSemaphore?.Dispose();
        _fileWatcherService.FileChanged -= OnFileChanged;
        base.Dispose();
    }
}
