namespace Mo5.RagServer.Core.Entities;

/// <summary>
/// Many-to-many relationship between Documents and Tags
/// </summary>
public class DocumentTag
{
    public Guid DocumentId { get; set; }
    public Guid TagId { get; set; }
    
    /// <summary>
    /// When this tag was assigned to the document
    /// </summary>
    public DateTime AssignedAt { get; set; }
    
    /// <summary>
    /// How the tag was assigned (e.g., "auto", "manual", "filename")
    /// </summary>
    public string AssignmentSource { get; set; } = "auto";
    
    /// <summary>
    /// Confidence score for auto-assigned tags (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; } = 1.0f;
    
    /// <summary>
    /// Navigation properties
    /// </summary>
    public virtual Document Document { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
