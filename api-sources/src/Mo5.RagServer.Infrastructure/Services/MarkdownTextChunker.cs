using System.Text.RegularExpressions;
using Mo5.RagServer.Core.Services;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Text chunker optimized for markdown documents
/// </summary>
public class MarkdownTextChunker : ITextChunker
{
    private static readonly Regex HeaderRegex = new(@"^(#{1,6})\s+(.+)$", RegexOptions.Multiline);
    private static readonly Regex CodeBlockRegex = new(@"```[\s\S]*?```", RegexOptions.Multiline);
    
    public List<TextChunk> ChunkText(string text, int chunkSize = 1000, int overlap = 200)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<TextChunk>();

        var chunks = new List<TextChunk>();
        var lines = text.Split('\n');
        var currentChunk = string.Empty;
        var currentStartPos = 0;
        var currentPos = 0;

        foreach (var line in lines)
        {
            var lineWithNewline = line + '\n';
            
            // If adding this line would exceed chunk size, finalize current chunk
            if (currentChunk.Length + lineWithNewline.Length > chunkSize && !string.IsNullOrWhiteSpace(currentChunk))
            {
                chunks.Add(new TextChunk
                {
                    Content = currentChunk.TrimEnd(),
                    StartPosition = currentStartPos,
                    EndPosition = currentPos,
                    EstimatedTokens = EstimateTokens(currentChunk)
                });

                // Start new chunk with overlap
                var overlapText = GetOverlapText(currentChunk, overlap);
                currentChunk = overlapText;
                currentStartPos = currentPos - overlapText.Length;
            }

            // If this is the first line of a new chunk, record start position
            if (string.IsNullOrWhiteSpace(currentChunk))
            {
                currentStartPos = currentPos;
            }

            currentChunk += lineWithNewline;
            currentPos += lineWithNewline.Length;
        }

        // Add final chunk if it has content
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(new TextChunk
            {
                Content = currentChunk.TrimEnd(),
                StartPosition = currentStartPos,
                EndPosition = currentPos,
                EstimatedTokens = EstimateTokens(currentChunk)
            });
        }

        return chunks;
    }

    public List<TextChunk> ChunkMarkdown(string markdownText, int chunkSize = 1000, int overlap = 200)
    {
        if (string.IsNullOrWhiteSpace(markdownText))
            return new List<TextChunk>();

        var chunks = new List<TextChunk>();
        var sections = SplitIntoSections(markdownText);

        foreach (var section in sections)
        {
            if (section.Content.Length <= chunkSize)
            {
                // Section fits in one chunk
                chunks.Add(new TextChunk
                {
                    Content = section.Content.Trim(),
                    StartPosition = section.StartPosition,
                    EndPosition = section.EndPosition,
                    SectionHeading = section.Heading,
                    EstimatedTokens = EstimateTokens(section.Content)
                });
            }
            else
            {
                // Section needs to be split into multiple chunks
                var sectionChunks = ChunkText(section.Content, chunkSize, overlap);
                foreach (var chunk in sectionChunks)
                {
                    chunk.StartPosition += section.StartPosition;
                    chunk.EndPosition += section.StartPosition;
                    chunk.SectionHeading = section.Heading;
                    chunks.Add(chunk);
                }
            }
        }

        return chunks;
    }

    private List<MarkdownSection> SplitIntoSections(string markdownText)
    {
        var sections = new List<MarkdownSection>();
        var headerMatches = HeaderRegex.Matches(markdownText).Cast<Match>().ToList();

        if (!headerMatches.Any())
        {
            // No headers found, treat entire text as one section
            sections.Add(new MarkdownSection
            {
                Heading = null,
                Content = markdownText,
                StartPosition = 0,
                EndPosition = markdownText.Length
            });
            return sections;
        }

        // Add content before first header as introduction
        if (headerMatches[0].Index > 0)
        {
            var introContent = markdownText.Substring(0, headerMatches[0].Index).Trim();
            if (!string.IsNullOrWhiteSpace(introContent))
            {
                sections.Add(new MarkdownSection
                {
                    Heading = "Introduction",
                    Content = introContent,
                    StartPosition = 0,
                    EndPosition = headerMatches[0].Index
                });
            }
        }

        // Process each header section
        for (int i = 0; i < headerMatches.Count; i++)
        {
            var currentMatch = headerMatches[i];
            var nextMatch = i + 1 < headerMatches.Count ? headerMatches[i + 1] : null;
            
            var startPos = currentMatch.Index;
            var endPos = nextMatch?.Index ?? markdownText.Length;
            var content = markdownText.Substring(startPos, endPos - startPos).Trim();
            
            if (!string.IsNullOrWhiteSpace(content))
            {
                sections.Add(new MarkdownSection
                {
                    Heading = currentMatch.Groups[2].Value.Trim(),
                    Content = content,
                    StartPosition = startPos,
                    EndPosition = endPos
                });
            }
        }

        return sections;
    }

    private string GetOverlapText(string text, int overlapSize)
    {
        if (text.Length <= overlapSize)
            return text;

        // Try to find a good break point (end of sentence or paragraph)
        var overlapStart = text.Length - overlapSize;
        var searchText = text.Substring(overlapStart);
        
        var sentenceEnd = searchText.LastIndexOfAny(new[] { '.', '!', '?' });
        if (sentenceEnd > 0 && sentenceEnd < searchText.Length - 1)
        {
            return searchText.Substring(sentenceEnd + 1).TrimStart();
        }

        var paragraphEnd = searchText.LastIndexOf('\n');
        if (paragraphEnd > 0)
        {
            return searchText.Substring(paragraphEnd + 1).TrimStart();
        }

        // Fallback to simple character-based overlap
        return text.Substring(overlapStart);
    }

    private int EstimateTokens(string text)
    {
        // Rough estimation: 1 token ≈ 4 characters for English text
        // This is a simplification, but good enough for our purposes
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private class MarkdownSection
    {
        public string? Heading { get; set; }
        public string Content { get; set; } = string.Empty;
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }
}
