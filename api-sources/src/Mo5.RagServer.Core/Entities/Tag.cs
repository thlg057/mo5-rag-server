namespace Mo5.RagServer.Core.Entities;

/// <summary>
/// Represents a tag for categorizing documents
/// </summary>
public class Tag
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tag name (e.g., "C", "Assembly", "text-mode", "graphics-mode", "basic")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Tag description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tag category (e.g., "language", "mode", "topic")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Display color for UI (hex color code)
    /// </summary>
    public string Color { get; set; } = "#6B7280";
    
    /// <summary>
    /// When this tag was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Whether this tag is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Documents associated with this tag
    /// </summary>
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
}
