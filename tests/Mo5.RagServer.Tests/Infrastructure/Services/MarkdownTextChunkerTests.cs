using FluentAssertions;
using Mo5.RagServer.Infrastructure.Services;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class MarkdownTextChunkerTests
{
    private readonly MarkdownTextChunker _chunker = new();

    [Fact]
    public void ChunkText_WithShortText_ReturnsOneChunk()
    {
        // Arrange
        var text = "This is a short text that should fit in one chunk.";

        // Act
        var chunks = _chunker.ChunkText(text, chunkSize: 1000);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Be(text);
        chunks[0].StartPosition.Should().Be(0);
        chunks[0].EndPosition.Should().Be(text.Length + 1); // +1 for newline
    }

    [Fact]
    public void ChunkText_WithLongText_ReturnsMultipleChunks()
    {
        // Arrange
        var text = string.Join("\n", Enumerable.Repeat("This is a line of text that will be repeated.", 50));

        // Act
        var chunks = _chunker.ChunkText(text, chunkSize: 500, overlap: 100);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.All(c => c.Content.Length <= 600).Should().BeTrue(); // Allow some flexibility for overlap
    }

    [Fact]
    public void ChunkMarkdown_WithHeaders_PreservesStructure()
    {
        // Arrange
        var markdown = @"# Main Title

This is the introduction paragraph.

## Section 1

Content for section 1 with some details.

### Subsection 1.1

More detailed content here.

## Section 2

Content for section 2.";

        // Act
        var chunks = _chunker.ChunkMarkdown(markdown, chunkSize: 200);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().Contain(c => c.SectionHeading == "Main Title");
        chunks.Should().Contain(c => c.SectionHeading == "Section 1");
        chunks.Should().Contain(c => c.SectionHeading == "Subsection 1.1");
        chunks.Should().Contain(c => c.SectionHeading == "Section 2");
    }

    [Fact]
    public void ChunkMarkdown_WithCodeBlocks_PreservesCodeIntegrity()
    {
        // Arrange
        var markdown = @"# Programming Example

Here's a C code example:

```c
#include <stdio.h>

int main() {
    printf(""Hello, MO5!"");
    return 0;
}
```

This code demonstrates basic C programming.";

        // Act
        var chunks = _chunker.ChunkMarkdown(markdown, chunkSize: 150);

        // Assert
        chunks.Should().HaveCountGreaterThan(0);
        // Code blocks should be preserved in chunks
        var codeChunk = chunks.FirstOrDefault(c => c.Content.Contains("```c"));
        codeChunk.Should().NotBeNull();
    }

    [Fact]
    public void ChunkText_EstimatesTokensCorrectly()
    {
        // Arrange
        var text = "This is a test text with approximately sixteen tokens in total.";

        // Act
        var chunks = _chunker.ChunkText(text);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].EstimatedTokens.Should().BeGreaterThan(10);
        chunks[0].EstimatedTokens.Should().BeLessThan(25);
    }

    [Fact]
    public void ChunkMarkdown_WithEmptyText_ReturnsEmptyList()
    {
        // Arrange
        var emptyText = "";

        // Act
        var chunks = _chunker.ChunkMarkdown(emptyText);

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void ChunkMarkdown_WithOnlyWhitespace_ReturnsEmptyList()
    {
        // Arrange
        var whitespaceText = "   \n\n   \t  \n  ";

        // Act
        var chunks = _chunker.ChunkMarkdown(whitespaceText);

        // Assert
        chunks.Should().BeEmpty();
    }
}
