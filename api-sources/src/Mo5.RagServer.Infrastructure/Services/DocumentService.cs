using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Models;
using Mo5.RagServer.Core.Services;
using Mo5.RagServer.Infrastructure.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Service for managing documents and performing semantic search
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly RagDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ITextChunker _textChunker;
    private readonly ITagDetectionService _tagDetectionService;
    private readonly ILogger<DocumentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IIngestionStatsService? _statsService;

    public DocumentService(
        RagDbContext context,
        IEmbeddingService embeddingService,
        ITextChunker textChunker,
        ITagDetectionService tagDetectionService,
        ILogger<DocumentService> logger,
        IConfiguration configuration,
        IIngestionStatsService? statsService = null)
    {
        _context = context;
        _embeddingService = embeddingService;
        _textChunker = textChunker;
        _tagDetectionService = tagDetectionService;
        _logger = logger;
        _configuration = configuration;
        _statsService = statsService;
    }

    public async Task<int> IndexDocumentsAsync(string knowledgeBasePath, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(knowledgeBasePath))
        {
            _logger.LogWarning("Knowledge base path does not exist: {Path}", knowledgeBasePath);
            return 0;
        }

        var markdownFiles = Directory.GetFiles(knowledgeBasePath, "*.md", SearchOption.AllDirectories);
        _logger.LogInformation("Found {Count} markdown files to index", markdownFiles.Length);

        var processedCount = 0;
        foreach (var filePath in markdownFiles)
        {
            try
            {
                await IndexDocumentAsync(filePath, cancellationToken);
                processedCount++;
                _logger.LogDebug("Indexed document: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index document: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Successfully indexed {ProcessedCount} out of {TotalCount} documents", 
            processedCount, markdownFiles.Length);

        return processedCount;
    }

    public async Task<Document> IndexDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var contentHash = ComputeContentHash(content);

        // Check if document already exists and is up to date
        var relativePath = GetRelativePath(filePath);
        var existingDocument = await _context.Documents
            .Include(d => d.Chunks)
            .Include(d => d.DocumentTags)
            .FirstOrDefaultAsync(d => d.FilePath == relativePath, cancellationToken);

        if (existingDocument != null && existingDocument.ContentHash == contentHash)
        {
            _logger.LogDebug("Document unchanged, skipping: {FilePath}", filePath);
            return existingDocument;
        }

        // Create or update document
        var document = existingDocument ?? new Document { Id = Guid.NewGuid() };
        var title = ExtractTitle(content) ?? Path.GetFileNameWithoutExtension(fileInfo.Name);

        document.FileName = fileInfo.Name;
        document.FilePath = relativePath;
        document.Title = title;
        document.Content = content;
        document.FileSize = fileInfo.Length;
        document.ContentHash = contentHash;
        document.LastModified = fileInfo.LastWriteTimeUtc;
        document.UpdatedAt = DateTime.UtcNow;
        document.IsActive = true;

        if (existingDocument == null)
        {
            document.CreatedAt = DateTime.UtcNow;
            _context.Documents.Add(document);
        }

        // Remove existing chunks if updating
        if (existingDocument?.Chunks.Any() == true)
        {
            _context.DocumentChunks.RemoveRange(existingDocument.Chunks);
        }

        // Generate chunks and embeddings
        await GenerateChunksAsync(document, cancellationToken);

        // Detect and assign tags
        await AssignTagsAsync(document, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

        _logger.LogInformation("Successfully indexed document: {Title} ({ChunkCount} chunks)",
            document.Title, document.Chunks.Count);

        // Record success statistics
        _statsService?.RecordIndexingSuccess(filePath, processingTime, document.Chunks.Count);

        return document;
        }
        catch (Exception ex)
        {
            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Failed to index document: {FilePath}", filePath);

            // Record failure statistics
            _statsService?.RecordIndexingFailure(filePath, ex.Message);

            throw;
        }
    }

    private string Cleanup(string textToCleanup)
    {
        return textToCleanup.Replace("#", "").Replace("**", "").Trim();
    }

    private async Task GenerateChunksAsync(Document document, CancellationToken cancellationToken)
    {
        var chunkSize = _configuration.GetValue<int>("RagSettings:ChunkSize", 1000);
        var chunkOverlap = _configuration.GetValue<int>("RagSettings:ChunkOverlap", 200);

        var textChunks = _textChunker.ChunkMarkdown(document.Content, chunkSize, chunkOverlap);
        
        if (!textChunks.Any())
        {
            _logger.LogWarning("No chunks generated for document: {Title}", document.Title);
            return;
        }

        // --- OPTIMISATION : Enrichissement sémantique ---
        // On prépare un texte enrichi pour l'embedding, mais on garde le contenu brut pour l'affichage
        var enrichedTexts = textChunks.Select(c => 
        {
            var cleanTitle = Cleanup(document.Title);
            var cleanSection = Cleanup(c.SectionHeading ?? "Général");
            
            // Ce format aide le modèle E5 à lier le contenu au sujet global du fichier
            return $"Document: {cleanTitle} > Section: {cleanSection}\nContenu: {Cleanup(c.Content)}";
        }).ToArray();

        // On génère les embeddings à partir des textes enrichis
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(enrichedTexts, cancellationToken);

        // Create DocumentChunk entities
        for (int i = 0; i < textChunks.Count; i++)
        {
            var textChunk = textChunks[i];
            var embedding = embeddings[i];

            var documentChunk = new DocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                ChunkIndex = i,
                // On nettoie les '#' du contenu stocké pour une réponse plus propre
                Content = Cleanup(textChunk.Content), 
                Embedding = embedding,
                StartPosition = textChunk.StartPosition,
                EndPosition = textChunk.EndPosition,
                Length = textChunk.Length,
                TokenCount = textChunk.EstimatedTokens,
                SectionHeading = Cleanup(textChunk.SectionHeading ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow
            };

            document.Chunks.Add(documentChunk);
        }

        _logger.LogDebug("Generated {ChunkCount} chunks for document: {Title}", 
            textChunks.Count, document.Title);
    }

    private async Task AssignTagsAsync(Document document, CancellationToken cancellationToken)
    {
        var availableTags = await _context.Tags.Where(t => t.IsActive).ToListAsync(cancellationToken);
        var detectedTags = await _tagDetectionService.DetectTagsAsync(
            document.FileName, document.Content, availableTags);

        // Remove existing tags if updating
        var existingDocumentTags = document.DocumentTags.ToList();
        foreach (var existingTag in existingDocumentTags)
        {
            document.DocumentTags.Remove(existingTag);
        }

        // Add detected tags
        foreach (var detectedTag in detectedTags)
        {
            var documentTag = new DocumentTag
            {
                DocumentId = document.Id,
                TagId = detectedTag.Tag.Id,
                AssignedAt = DateTime.UtcNow,
                AssignmentSource = detectedTag.Source,
                Confidence = detectedTag.Confidence
            };

            document.DocumentTags.Add(documentTag);
        }

        _logger.LogDebug("Assigned {TagCount} tags to document: {Title}", 
            detectedTags.Count, document.Title);
    }

    private string ComputeContentHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private string GetRelativePath(string fullPath)
    {
        var knowledgeBasePath = _configuration.GetValue<string>("RagSettings:KnowledgeBasePath", "./knowledge") ?? "./knowledge";
        var fullKnowledgeBasePath = Path.GetFullPath(knowledgeBasePath);
        var fullFilePath = Path.GetFullPath(fullPath);

        if (fullFilePath.StartsWith(fullKnowledgeBasePath))
        {
            return Path.GetRelativePath(fullKnowledgeBasePath, fullFilePath).Replace('\\', '/');
        }

        return Path.GetFileName(fullPath);
    }

    private string? ExtractTitle(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // Look for first H1 header
        var h1Line = lines.FirstOrDefault(line => line.TrimStart().StartsWith("# "));
        if (h1Line != null)
        {
            return h1Line.TrimStart().Substring(2).Trim();
        }

        // Look for first non-empty line as title
        var firstLine = lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line.Trim()));
        if (firstLine != null && firstLine.Length < 100)
        {
            return firstLine.Trim().TrimStart('#').Trim();
        }

        return null;
    }

    public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return new SearchResponse
            {
                Query = request.Query,
                Results = new List<SearchResult>(),
                TotalResults = 0,
                ExecutionTimeMs = 0,
                Filters = new SearchFilters
                {
                    Tags = request.Tags,
                    MinSimilarityScore = request.MinSimilarityScore,
                    MaxResults = request.MaxResults
                }
            };
        }

        var startTime = DateTime.UtcNow;

        try
        {
            // Generate embedding for the search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Query, cancellationToken);

            // Build the query
            var query = _context.DocumentChunks
                .Include(dc => dc.Document)
                .ThenInclude(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
                .Where(dc => dc.Document.IsActive);

            // Apply tag filters if specified
            if (request.Tags.Any())
            {
                query = query.Where(dc => dc.Document.DocumentTags
                    .Any(dt => request.Tags.Contains(dt.Tag.Name) && dt.Tag.IsActive));
            }

            // Perform vector similarity search
            // Note: We fetch more results and sort in memory due to pgvector API limitations
            var chunks = await query
                .Take(1000) // Fetch a reasonable number of chunks
                .ToListAsync(cancellationToken);

            // Sort by similarity in memory
            chunks = chunks
                .OrderByDescending(dc => CalculateCosineSimilarity(dc.Embedding.ToArray(), queryEmbedding.ToArray()))
                .Take(request.MaxResults * 2)
                .ToList();

            // Filter by similarity score and convert to search results
            var results = chunks
                .Select(chunk => new
                {
                    Chunk = chunk,
                    SimilarityScore = CalculateCosineSimilarity(chunk.Embedding.ToArray(), queryEmbedding.ToArray())
                })
                .Where(x => x.SimilarityScore >= request.MinSimilarityScore)
                .Take(request.MaxResults)
                .Select(x => CreateSearchResult(x.Chunk, x.SimilarityScore, request.IncludeMetadata, request.IncludeContext))
                .ToList();

            var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new SearchResponse
            {
                Query = request.Query,
                Results = results,
                TotalResults = results.Count(),
                ExecutionTimeMs = executionTime,
                Filters = new SearchFilters
                {
                    Tags = request.Tags,
                    MinSimilarityScore = request.MinSimilarityScore,
                    MaxResults = request.MaxResults
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform semantic search for query: {Query}", request.Query);
            throw;
        }
    }

    public async Task<List<Document>> GetDocumentsAsync(List<string>? tags = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Documents
            .Include(d => d.DocumentTags)
            .ThenInclude(dt => dt.Tag)
            .Where(d => d.IsActive);

        if (tags?.Any() == true)
        {
            query = query.Where(d => d.DocumentTags
                .Any(dt => tags.Contains(dt.Tag.Name) && dt.Tag.IsActive));
        }

        return await query
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Chunks)
            .Include(d => d.DocumentTags)
            .ThenInclude(dt => dt.Tag)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.IsActive, cancellationToken);
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document == null)
            return false;

        document.IsActive = false;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<Tag>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    private SearchResult CreateSearchResult(DocumentChunk chunk, float similarityScore, bool includeMetadata, bool includeContext)
    {
        var result = new SearchResult
        {
            ChunkId = chunk.Id,
            Content = chunk.Content,
            SimilarityScore = similarityScore,
            Position = new ChunkPosition
            {
                ChunkIndex = chunk.ChunkIndex,
                StartPosition = chunk.StartPosition,
                EndPosition = chunk.EndPosition,
                SectionHeading = chunk.SectionHeading
            }
        };

        if (includeMetadata)
        {
            result.Document = new DocumentMetadata
            {
                DocumentId = chunk.Document.Id,
                FileName = chunk.Document.FileName,
                Title = chunk.Document.Title,
                FilePath = chunk.Document.FilePath,
                LastModified = chunk.Document.LastModified,
                Tags = chunk.Document.DocumentTags
                    .Where(dt => dt.Tag.IsActive)
                    .Select(dt => dt.Tag.Name)
                    .ToList()
            };
        }

        if (includeContext)
        {
            result.Context = GetChunkContext(chunk);
        }

        return result;
    }

    private string? GetChunkContext(DocumentChunk chunk)
    {
        // Get surrounding chunks for context
        var contextChunks = _context.DocumentChunks
            .Where(dc => dc.DocumentId == chunk.DocumentId)
            .Where(dc => Math.Abs(dc.ChunkIndex - chunk.ChunkIndex) <= 1)
            .OrderBy(dc => dc.ChunkIndex)
            .Select(dc => dc.Content)
            .ToList();

        if (contextChunks.Count <= 1)
            return null;

        return string.Join("\n\n...\n\n", contextChunks);
    }

    private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            return 0f;

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0f;

        return (float)(dotProduct / (magnitudeA * magnitudeB));
    }
}
