using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Core.Services;

/// <summary>
/// Service for automatically detecting tags from document content
/// </summary>
public interface ITagDetectionService
{
    /// <summary>
    /// Detect tags from document filename and content
    /// </summary>
    /// <param name="fileName">Document filename</param>
    /// <param name="content">Document content</param>
    /// <param name="availableTags">Available tags to choose from</param>
    /// <returns>List of detected tags with confidence scores</returns>
    Task<List<DetectedTag>> DetectTagsAsync(string fileName, string content, List<Tag> availableTags);
}

/// <summary>
/// Represents a detected tag with confidence score
/// </summary>
public class DetectedTag
{
    public Tag Tag { get; set; } = null!;
    public float Confidence { get; set; }
    public string Source { get; set; } = "auto";
    public string Reason { get; set; } = string.Empty;
}
