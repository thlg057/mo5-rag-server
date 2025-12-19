using Pgvector;

namespace Mo5.RagServer.Core.Entities;

/// <summary>
/// Represents a chunk of a document with its vector embedding
/// </summary>
public class DocumentChunk
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to the parent document
    /// </summary>
    public Guid DocumentId { get; set; }
    
    /// <summary>
    /// Order of this chunk within the document (0-based)
    /// </summary>
    public int ChunkIndex { get; set; }
    
    /// <summary>
    /// The text content of this chunk
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Vector embedding of the chunk content (dimensions vary by embedding service)
    /// - TF-IDF: 384 dimensions
    /// - Local Sentence Transformers: 384 dimensions
    /// - OpenAI text-embedding-3-small: 1536 dimensions
    /// </summary>
    public Vector Embedding { get; set; } = new(Array.Empty<float>());
    
    /// <summary>
    /// Start position of this chunk in the original document
    /// </summary>
    public int StartPosition { get; set; }
    
    /// <summary>
    /// End position of this chunk in the original document
    /// </summary>
    public int EndPosition { get; set; }
    
    /// <summary>
    /// Length of the chunk in characters
    /// </summary>
    public int Length { get; set; }
    
    /// <summary>
    /// Number of tokens in this chunk (approximate)
    /// </summary>
    public int TokenCount { get; set; }
    
    /// <summary>
    /// Section heading or context where this chunk appears
    /// </summary>
    public string? SectionHeading { get; set; }
    
    /// <summary>
    /// When this chunk was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Navigation property to parent document
    /// </summary>
    public virtual Document Document { get; set; } = null!;
}
