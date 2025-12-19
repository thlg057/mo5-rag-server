namespace Mo5.RagServer.Core.Models;

/// <summary>
/// Request model for semantic search
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// The search query text
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum number of results to return (default: 10, max: 50)
    /// </summary>
    public int MaxResults { get; set; } = 10;
    
    /// <summary>
    /// Minimum similarity score threshold (0.0 to 1.0, default: 0.7)
    /// </summary>
    public float MinSimilarityScore { get; set; } = 0.7f;
    
    /// <summary>
    /// Filter by specific tags (optional)
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Include document metadata in response
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
    
    /// <summary>
    /// Include chunk context (surrounding text)
    /// </summary>
    public bool IncludeContext { get; set; } = false;
}
