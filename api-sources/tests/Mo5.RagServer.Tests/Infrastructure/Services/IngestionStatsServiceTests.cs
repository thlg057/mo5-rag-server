using FluentAssertions;
using Mo5.RagServer.Infrastructure.Services;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class IngestionStatsServiceTests
{
    private readonly IngestionStatsService _statsService;

    public IngestionStatsServiceTests()
    {
        _statsService = new IngestionStatsService();
    }

    [Fact]
    public void RecordIndexingSuccess_UpdatesStats()
    {
        // Arrange
        var filePath = "test.md";
        var processingTime = 150L;
        var chunkCount = 5;

        // Act
        _statsService.RecordIndexingSuccess(filePath, processingTime, chunkCount);

        // Assert
        var stats = _statsService.GetStats();
        stats.TotalFilesProcessed.Should().Be(1);
        stats.SuccessfulIndexings.Should().Be(1);
        stats.FailedIndexings.Should().Be(0);
        stats.TotalProcessingTimeMs.Should().Be(processingTime);
        stats.TotalChunksCreated.Should().Be(chunkCount);
        stats.SuccessRate.Should().Be(100.0);
        stats.AverageProcessingTimeMs.Should().Be(processingTime);
        stats.LastProcessedAt.Should().NotBeNull();
        stats.RecentActivities.Should().HaveCount(1);
        stats.RecentActivities.First().Success.Should().BeTrue();
    }

    [Fact]
    public void RecordIndexingFailure_UpdatesStats()
    {
        // Arrange
        var filePath = "test.md";
        var error = "Test error message";

        // Act
        _statsService.RecordIndexingFailure(filePath, error);

        // Assert
        var stats = _statsService.GetStats();
        stats.TotalFilesProcessed.Should().Be(1);
        stats.SuccessfulIndexings.Should().Be(0);
        stats.FailedIndexings.Should().Be(1);
        stats.SuccessRate.Should().Be(0.0);
        stats.LastFailureAt.Should().NotBeNull();
        stats.LastError.Should().Be(error);
        stats.RecentActivities.Should().HaveCount(1);
        stats.RecentActivities.First().Success.Should().BeFalse();
        stats.RecentActivities.First().Error.Should().Be(error);
    }

    [Fact]
    public void MultipleOperations_CalculatesCorrectStats()
    {
        // Arrange & Act
        _statsService.RecordIndexingSuccess("file1.md", 100, 3);
        _statsService.RecordIndexingSuccess("file2.md", 200, 5);
        _statsService.RecordIndexingFailure("file3.md", "Error");
        _statsService.RecordIndexingSuccess("file4.md", 150, 4);

        // Assert
        var stats = _statsService.GetStats();
        stats.TotalFilesProcessed.Should().Be(4);
        stats.SuccessfulIndexings.Should().Be(3);
        stats.FailedIndexings.Should().Be(1);
        stats.SuccessRate.Should().Be(75.0);
        stats.TotalProcessingTimeMs.Should().Be(450); // 100 + 200 + 150
        stats.AverageProcessingTimeMs.Should().Be(150.0); // 450 / 3
        stats.TotalChunksCreated.Should().Be(12); // 3 + 5 + 4
        stats.RecentActivities.Should().HaveCount(4);
    }

    [Fact]
    public void ResetStats_ClearsAllData()
    {
        // Arrange
        _statsService.RecordIndexingSuccess("file1.md", 100, 3);
        _statsService.RecordIndexingFailure("file2.md", "Error");

        // Act
        _statsService.ResetStats();

        // Assert
        var stats = _statsService.GetStats();
        stats.TotalFilesProcessed.Should().Be(0);
        stats.SuccessfulIndexings.Should().Be(0);
        stats.FailedIndexings.Should().Be(0);
        stats.TotalProcessingTimeMs.Should().Be(0);
        stats.TotalChunksCreated.Should().Be(0);
        stats.LastProcessedAt.Should().BeNull();
        stats.LastFailureAt.Should().BeNull();
        stats.LastError.Should().BeNull();
        stats.RecentActivities.Should().BeEmpty();
    }

    [Fact]
    public void RecentActivities_LimitsToMaximumCount()
    {
        // Arrange & Act - Add more than the maximum number of activities
        for (int i = 0; i < 150; i++)
        {
            _statsService.RecordIndexingSuccess($"file{i}.md", 100, 1);
        }

        // Assert
        var stats = _statsService.GetStats();
        stats.RecentActivities.Should().HaveCount(100); // Should be limited to max
        stats.TotalFilesProcessed.Should().Be(150); // But total count should be correct
    }

    [Fact]
    public void GetStats_WithNoData_ReturnsEmptyStats()
    {
        // Act
        var stats = _statsService.GetStats();

        // Assert
        stats.TotalFilesProcessed.Should().Be(0);
        stats.SuccessfulIndexings.Should().Be(0);
        stats.FailedIndexings.Should().Be(0);
        stats.SuccessRate.Should().Be(0.0);
        stats.AverageProcessingTimeMs.Should().Be(0.0);
        stats.TotalProcessingTimeMs.Should().Be(0);
        stats.TotalChunksCreated.Should().Be(0);
        stats.LastProcessedAt.Should().BeNull();
        stats.LastFailureAt.Should().BeNull();
        stats.LastError.Should().BeNull();
        stats.RecentActivities.Should().BeEmpty();
    }

    [Fact]
    public async Task ConcurrentOperations_HandledSafely()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Perform concurrent operations
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() => _statsService.RecordIndexingSuccess($"file{index}.md", 100, 1)));
        }

        await Task.WhenAll(tasks);

        // Assert
        var stats = _statsService.GetStats();
        stats.TotalFilesProcessed.Should().Be(10);
        stats.SuccessfulIndexings.Should().Be(10);
        stats.RecentActivities.Should().HaveCount(10);
    }
}
