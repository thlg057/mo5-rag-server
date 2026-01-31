using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Infrastructure.Services;
using Moq;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class LocalEmbeddingServiceTests
{
    private readonly Mock<ILogger<LocalEmbeddingService>> _mockLogger;
    private readonly IConfiguration _configuration;

    public LocalEmbeddingServiceTests()
    {
        _mockLogger = new Mock<ILogger<LocalEmbeddingService>>();

        // Use real configuration with in-memory values
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LocalEmbedding:ModelName"] = "paraphrase-multilingual-MiniLM-L12-v2",
            ["LocalEmbedding:PythonPath"] = "python"
        });
        _configuration = configurationBuilder.Build();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Act
        var service = new LocalEmbeddingService(_mockLogger.Object, _configuration);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithNullText_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new LocalEmbeddingService(_mockLogger.Object, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateEmbeddingAsync(null!));
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithEmptyText_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new LocalEmbeddingService(_mockLogger.Object, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateEmbeddingAsync(""));
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var service = new LocalEmbeddingService(_mockLogger.Object, _configuration);

        // Act
        var result = await service.GenerateEmbeddingsAsync(Array.Empty<string>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var service = new LocalEmbeddingService(_mockLogger.Object, _configuration);

        // Act & Assert
        service.Dispose(); // Should not throw
    }
}
