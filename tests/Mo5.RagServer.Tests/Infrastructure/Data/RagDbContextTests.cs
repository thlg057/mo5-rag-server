using Microsoft.EntityFrameworkCore;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Infrastructure.Data;
using Pgvector;
using Xunit;
using FluentAssertions;

namespace Mo5.RagServer.Tests.Infrastructure.Data;

/// <summary>
/// Tests for RagDbContext
/// NOTE: These tests require PostgreSQL with pgvector extension and are skipped by default.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class RagDbContextTests : IDisposable
{
    private readonly RagDbContext _context;

    public RagDbContextTests()
    {
        var options = new DbContextOptionsBuilder<RagDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RagDbContext(options);
    }

    [Fact]
    public async Task CanCreateAndRetrieveDocument()
    {
        // Arrange
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "test.md",
            FilePath = "knowledge/test.md",
            Title = "Test Document",
            Content = "This is a test document content.",
            FileSize = 100,
            ContentHash = "abc123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        var retrievedDocument = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        // Assert
        retrievedDocument.Should().NotBeNull();
        retrievedDocument!.FileName.Should().Be("test.md");
        retrievedDocument.Title.Should().Be("Test Document");
        retrievedDocument.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateDocumentWithChunks()
    {
        // Arrange
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "test-with-chunks.md",
            FilePath = "knowledge/test-with-chunks.md",
            Title = "Test Document with Chunks",
            Content = "This is a longer test document that will be split into chunks.",
            FileSize = 200,
            ContentHash = "def456",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            ChunkIndex = 0,
            Content = "This is a longer test document",
            Embedding = new Vector(new float[1536]), // Empty embedding for test
            StartPosition = 0,
            EndPosition = 30,
            Length = 30,
            TokenCount = 8,
            SectionHeading = "Introduction",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Documents.Add(document);
        _context.DocumentChunks.Add(chunk);
        await _context.SaveChangesAsync();

        var retrievedDocument = await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        // Assert
        retrievedDocument.Should().NotBeNull();
        retrievedDocument!.Chunks.Should().HaveCount(1);
        retrievedDocument.Chunks.First().Content.Should().Be("This is a longer test document");
        retrievedDocument.Chunks.First().ChunkIndex.Should().Be(0);
    }

    [Fact]
    public async Task CanCreateTagsAndAssignToDocument()
    {
        // Arrange
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "c-example.md",
            FilePath = "knowledge/c-example.md",
            Title = "C Programming Example",
            Content = "Example C code for MO5",
            FileSize = 150,
            ContentHash = "ghi789",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "C",
            Description = "C programming language",
            Category = "language",
            Color = "#00599C",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var documentTag = new DocumentTag
        {
            DocumentId = document.Id,
            TagId = tag.Id,
            AssignedAt = DateTime.UtcNow,
            AssignmentSource = "auto",
            Confidence = 0.95f
        };

        // Act
        _context.Documents.Add(document);
        _context.Tags.Add(tag);
        _context.DocumentTags.Add(documentTag);
        await _context.SaveChangesAsync();

        var retrievedDocument = await _context.Documents
            .Include(d => d.DocumentTags)
            .ThenInclude(dt => dt.Tag)
            .FirstOrDefaultAsync(d => d.Id == document.Id);

        // Assert
        retrievedDocument.Should().NotBeNull();
        retrievedDocument!.DocumentTags.Should().HaveCount(1);
        retrievedDocument.DocumentTags.First().Tag.Name.Should().Be("C");
        retrievedDocument.DocumentTags.First().Confidence.Should().Be(0.95f);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
