namespace Mo5.RagServer.Core.Services;

/// <summary>
/// Service for splitting text into chunks for embedding
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Split text into chunks with overlap
    /// </summary>
    /// <param name="text">Text to split</param>
    /// <param name="chunkSize">Maximum chunk size in characters</param>
    /// <param name="overlap">Overlap between chunks in characters</param>
    /// <returns>List of text chunks with position information</returns>
    List<TextChunk> ChunkText(string text, int chunkSize = 1000, int overlap = 200);
    
    /// <summary>
    /// Split markdown text preserving structure
    /// </summary>
    /// <param name="markdownText">Markdown text to split</param>
    /// <param name="chunkSize">Maximum chunk size in characters</param>
    /// <param name="overlap">Overlap between chunks in characters</param>
    /// <returns>List of text chunks with markdown context</returns>
    List<TextChunk> ChunkMarkdown(string markdownText, int chunkSize = 1000, int overlap = 200);
}

/// <summary>
/// Represents a chunk of text with position and context information
/// </summary>
public class TextChunk
{
    public string Content { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int Length => EndPosition - StartPosition;
    public string? SectionHeading { get; set; }
    public int EstimatedTokens { get; set; }
}
