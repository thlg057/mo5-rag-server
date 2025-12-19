using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Models;
using Mo5.RagServer.Core.Services;
using Mo5.RagServer.Infrastructure.Data;
using Mo5.RagServer.Infrastructure.Services;
using Moq;
using Pgvector;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

/// <summary>
/// Tests for DocumentService search functionality
/// NOTE: These tests require PostgreSQL with pgvector extension and are skipped by default.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class DocumentServiceSearchTests : IDisposable
{
    private readonly RagDbContext _context;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<ITextChunker> _mockTextChunker;
    private readonly Mock<ITagDetectionService> _mockTagDetectionService;
    private readonly Mock<ILogger<DocumentService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly DocumentService _documentService;

    public DocumentServiceSearchTests()
    {
        var options = new DbContextOptionsBuilder<RagDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RagDbContext(options);
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockTextChunker = new Mock<ITextChunker>();
        _mockTagDetectionService = new Mock<ITagDetectionService>();
        _mockLogger = new Mock<ILogger<DocumentService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _documentService = new DocumentService(
            _context,
            _mockEmbeddingService.Object,
            _mockTextChunker.Object,
            _mockTagDetectionService.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);

        SetupTestData();
    }

    private void SetupTestData()
    {
        // Create test tags
        var cTag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "C",
            Category = "language",
            Color = "#00599C",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var assemblyTag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "Assembly",
            Category = "language",
            Color = "#6E4C13",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Tags.AddRange(cTag, assemblyTag);

        // Create test documents
        var document1 = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "c-programming.md",
            FilePath = "c-programming.md",
            Title = "C Programming Guide",
            Content = "Guide to C programming on MO5",
            FileSize = 100,
            ContentHash = "hash1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        var document2 = new Document
        {
            Id = Guid.NewGuid(),
            FileName = "assembly-guide.md",
            FilePath = "assembly-guide.md",
            Title = "Assembly Programming",
            Content = "Guide to assembly programming",
            FileSize = 150,
            ContentHash = "hash2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        _context.Documents.AddRange(document1, document2);

        // Create test chunks with embeddings
        var chunk1 = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = document1.Id,
            ChunkIndex = 0,
            Content = "C programming basics for MO5",
            Embedding = new Vector(new float[1536]), // Mock embedding
            StartPosition = 0,
            EndPosition = 29,
            Length = 29,
            TokenCount = 6,
            CreatedAt = DateTime.UtcNow
        };

        var chunk2 = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = document2.Id,
            ChunkIndex = 0,
            Content = "Assembly language programming on 6809",
            Embedding = new Vector(new float[1536]), // Mock embedding
            StartPosition = 0,
            EndPosition = 38,
            Length = 38,
            TokenCount = 7,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentChunks.AddRange(chunk1, chunk2);

        // Create document-tag relationships
        var docTag1 = new DocumentTag
        {
            DocumentId = document1.Id,
            TagId = cTag.Id,
            AssignedAt = DateTime.UtcNow,
            AssignmentSource = "auto",
            Confidence = 0.9f
        };

        var docTag2 = new DocumentTag
        {
            DocumentId = document2.Id,
            TagId = assemblyTag.Id,
            AssignedAt = DateTime.UtcNow,
            AssignmentSource = "auto",
            Confidence = 0.95f
        };

        _context.DocumentTags.AddRange(docTag1, docTag2);
        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var queryEmbedding = new Vector(new float[1536]);
        _mockEmbeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryEmbedding);

        var request = new SearchRequest
        {
            Query = "C programming",
            MaxResults = 10,
            MinSimilarityScore = 0.0f,
            IncludeMetadata = true
        };

        // Act
        var response = await _documentService.SearchAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Query.Should().Be("C programming");
        response.Results.Should().NotBeEmpty();
        response.ExecutionTimeMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SearchAsync_WithTagFilter_ReturnsFilteredResults()
    {
        // Arrange
        var queryEmbedding = new Vector(new float[1536]);
        _mockEmbeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryEmbedding);

        var request = new SearchRequest
        {
            Query = "programming",
            MaxResults = 10,
            MinSimilarityScore = 0.0f,
            Tags = new List<string> { "C" },
            IncludeMetadata = true
        };

        // Act
        var response = await _documentService.SearchAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Results.Should().NotBeEmpty();
        response.Results.Should().OnlyContain(r => r.Document.Tags.Contains("C"));
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            MaxResults = 10,
            MinSimilarityScore = 0.7f
        };

        // Act
        var response = await _documentService.SearchAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Query.Should().Be("");
        response.Results.Should().BeEmpty();
        response.TotalResults.Should().Be(0);
    }

    [Fact]
    public async Task GetDocumentsAsync_WithoutFilter_ReturnsAllDocuments()
    {
        // Act
        var documents = await _documentService.GetDocumentsAsync();

        // Assert
        documents.Should().HaveCount(2);
        documents.Should().OnlyContain(d => d.IsActive);
    }

    [Fact]
    public async Task GetDocumentsAsync_WithTagFilter_ReturnsFilteredDocuments()
    {
        // Act
        var documents = await _documentService.GetDocumentsAsync(new List<string> { "C" });

        // Assert
        documents.Should().HaveCount(1);
        documents.First().Title.Should().Be("C Programming Guide");
    }

    [Fact]
    public async Task GetDocumentAsync_WithValidId_ReturnsDocument()
    {
        // Arrange
        var documentId = _context.Documents.First().Id;

        // Act
        var document = await _documentService.GetDocumentAsync(documentId);

        // Assert
        document.Should().NotBeNull();
        document!.Id.Should().Be(documentId);
        document.Chunks.Should().NotBeEmpty();
        document.DocumentTags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDocumentAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var document = await _documentService.GetDocumentAsync(invalidId);

        // Assert
        document.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithValidId_SoftDeletesDocument()
    {
        // Arrange
        var documentId = _context.Documents.First().Id;

        // Act
        var result = await _documentService.DeleteDocumentAsync(documentId);

        // Assert
        result.Should().BeTrue();
        
        var document = await _context.Documents.FindAsync(documentId);
        document.Should().NotBeNull();
        document!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetTagsAsync_ReturnsActiveTags()
    {
        // Act
        var tags = await _documentService.GetTagsAsync();

        // Assert
        tags.Should().HaveCount(2);
        tags.Should().OnlyContain(t => t.IsActive);
        tags.Should().Contain(t => t.Name == "C");
        tags.Should().Contain(t => t.Name == "Assembly");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
