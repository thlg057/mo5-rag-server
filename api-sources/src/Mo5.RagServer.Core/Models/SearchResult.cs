namespace Mo5.RagServer.Core.Models;

/// <summary>
/// Response model for semantic search
/// </summary>
public class SearchResponse
{
    /// <summary>
    /// The original search query
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Search results ordered by relevance
    /// </summary>
    public List<SearchResult> Results { get; set; } = new();
    
    /// <summary>
    /// Total number of results found
    /// </summary>
    public int TotalResults { get; set; }
    
    /// <summary>
    /// Time taken to execute the search in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// Applied filters
    /// </summary>
    public SearchFilters Filters { get; set; } = new();
}

/// <summary>
/// Individual search result
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Unique identifier for this chunk
    /// </summary>
    public Guid ChunkId { get; set; }
    
    /// <summary>
    /// The relevant text content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Similarity score (0.0 to 1.0)
    /// </summary>
    public float SimilarityScore { get; set; }
    
    /// <summary>
    /// Document metadata
    /// </summary>
    public DocumentMetadata Document { get; set; } = new();
    
    /// <summary>
    /// Position information within the document
    /// </summary>
    public ChunkPosition Position { get; set; } = new();
    
    /// <summary>
    /// Additional context around this chunk (if requested)
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// Document metadata for search results
/// </summary>
public class DocumentMetadata
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Position information for a chunk within its document
/// </summary>
public class ChunkPosition
{
    public int ChunkIndex { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public string? SectionHeading { get; set; }
}

/// <summary>
/// Applied search filters
/// </summary>
public class SearchFilters
{
    public List<string> Tags { get; set; } = new();
    public float MinSimilarityScore { get; set; }
    public int MaxResults { get; set; }
}
