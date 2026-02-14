using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Infrastructure.Data;
using Mo5.RagServer.Infrastructure.Services;
using Moq;
using Pgvector;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class DocumentServiceIndexingTests
{
    [Fact]
    public async Task IndexDocumentAsync_WithNestedFolders_AssignsFolderTags_AndCreatesSingleChunk()
    {
        var root = Path.Combine(Path.GetTempPath(), "mo5-rag-tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(root, "thomson", "mo5", "my-doc.md");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "# My doc\n\nSome content.");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RagSettings:KnowledgeBasePath"] = root
                })
                .Build();

            var options = new DbContextOptionsBuilder<RagDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RagDbContext(options);

            var embeddingService = new Mock<IEmbeddingService>();
            embeddingService
                .Setup(s => s.GenerateEmbeddingsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new Vector(new float[384]) });

            var logger = new Mock<ILogger<DocumentService>>();

            var service = new DocumentService(
                context,
                embeddingService.Object,
                logger.Object,
                configuration);

            // Act
            var document = await service.IndexDocumentAsync(filePath);

            // Assert: no split => 1 chunk
            document.Chunks.Should().HaveCount(1);
            document.Chunks.Single().ChunkIndex.Should().Be(0);

            // Assert: tags derived from folders
            document.FilePath.Should().Be("thomson/mo5/my-doc.md");
            document.DocumentTags.Should().HaveCount(2);
            document.DocumentTags.Should().OnlyContain(dt => dt.AssignmentSource == "path" && dt.Confidence == 1.0f);

            var tags = await context.Tags.OrderBy(t => t.Name).ToListAsync();
            tags.Select(t => (t.Name, t.Category)).Should().BeEquivalentTo(new[]
            {
                ("mo5", "folder"),
                ("thomson", "folder")
            });

            embeddingService.Verify(
                s => s.GenerateEmbeddingsAsync(It.Is<string[]>(arr => arr.Length == 1), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
