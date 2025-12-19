namespace Mo5.RagServer.Core.Entities;

/// <summary>
/// Represents a source document in the knowledge base
/// </summary>
public class Document
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Original filename (e.g., "assembly-graphics-mode.md")
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative path from knowledge base root
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Document title extracted from content or filename
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Full content of the document
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// SHA256 hash of the content for change detection
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;
    
    /// <summary>
    /// When the document was first indexed
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the document was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// When the source file was last modified
    /// </summary>
    public DateTime LastModified { get; set; }
    
    /// <summary>
    /// Whether the document is currently active/indexed
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Document chunks for vector search
    /// </summary>
    public virtual ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    
    /// <summary>
    /// Tags associated with this document
    /// </summary>
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
}
