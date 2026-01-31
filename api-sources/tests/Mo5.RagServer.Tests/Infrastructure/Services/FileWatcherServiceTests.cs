using FluentAssertions;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Services;
using Mo5.RagServer.Infrastructure.Services;
using Moq;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class FileWatcherServiceTests : IDisposable
{
    private readonly Mock<ILogger<FileWatcherService>> _mockLogger;
    private readonly FileWatcherService _fileWatcherService;
    private readonly string _testDirectory;

    public FileWatcherServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileWatcherService>>();
        _fileWatcherService = new FileWatcherService(_mockLogger.Object);
        
        // Create a temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task StartWatchingAsync_WithValidPath_StartsWatching()
    {
        // Act
        await _fileWatcherService.StartWatchingAsync(_testDirectory);

        // Assert
        _fileWatcherService.IsWatching.Should().BeTrue();
    }

    [Fact]
    public async Task StartWatchingAsync_WithInvalidPath_ThrowsException()
    {
        // Arrange
        var invalidPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => _fileWatcherService.StartWatchingAsync(invalidPath));
    }

    [Fact]
    public async Task StopWatchingAsync_WhenWatching_StopsWatching()
    {
        // Arrange
        await _fileWatcherService.StartWatchingAsync(_testDirectory);
        _fileWatcherService.IsWatching.Should().BeTrue();

        // Act
        await _fileWatcherService.StopWatchingAsync();

        // Assert
        _fileWatcherService.IsWatching.Should().BeFalse();
    }

    [Fact]
    public async Task FileChanged_WhenFileCreated_FiresEvent()
    {
        // Arrange
        var eventFired = false;
        FileChangeEventArgs? capturedArgs = null;

        _fileWatcherService.FileChanged += (sender, args) =>
        {
            eventFired = true;
            capturedArgs = args;
        };

        await _fileWatcherService.StartWatchingAsync(_testDirectory);

        // Act
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "Test content");

        // Wait a bit for the file system event to fire
        await Task.Delay(500);

        // Assert
        eventFired.Should().BeTrue();
        capturedArgs.Should().NotBeNull();
        // File system may report Created or Modified for new files depending on the OS
        capturedArgs!.ChangeType.Should().BeOneOf(FileChangeType.Created, FileChangeType.Modified);
        capturedArgs.FilePath.Should().Be(testFile);
    }

    [Fact]
    public async Task FileChanged_WhenFileModified_FiresEvent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "Initial content");

        var eventFired = false;
        FileChangeEventArgs? capturedArgs = null;

        _fileWatcherService.FileChanged += (sender, args) =>
        {
            if (args.ChangeType == FileChangeType.Modified)
            {
                eventFired = true;
                capturedArgs = args;
            }
        };

        await _fileWatcherService.StartWatchingAsync(_testDirectory);

        // Act
        await File.WriteAllTextAsync(testFile, "Modified content");

        // Wait a bit for the file system event to fire
        await Task.Delay(500);

        // Assert
        eventFired.Should().BeTrue();
        capturedArgs.Should().NotBeNull();
        capturedArgs!.ChangeType.Should().Be(FileChangeType.Modified);
    }

    [Fact]
    public async Task FileChanged_WhenFileDeleted_FiresEvent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.md");
        await File.WriteAllTextAsync(testFile, "Test content");

        var eventFired = false;
        FileChangeEventArgs? capturedArgs = null;

        _fileWatcherService.FileChanged += (sender, args) =>
        {
            if (args.ChangeType == FileChangeType.Deleted)
            {
                eventFired = true;
                capturedArgs = args;
            }
        };

        await _fileWatcherService.StartWatchingAsync(_testDirectory);

        // Act
        File.Delete(testFile);

        // Wait a bit for the file system event to fire
        await Task.Delay(500);

        // Assert
        eventFired.Should().BeTrue();
        capturedArgs.Should().NotBeNull();
        capturedArgs!.ChangeType.Should().Be(FileChangeType.Deleted);
    }

    [Fact]
    public async Task StartWatchingAsync_WhenAlreadyWatching_DoesNotThrow()
    {
        // Arrange
        await _fileWatcherService.StartWatchingAsync(_testDirectory);
        _fileWatcherService.IsWatching.Should().BeTrue();

        // Act & Assert
        await _fileWatcherService.StartWatchingAsync(_testDirectory);
        _fileWatcherService.IsWatching.Should().BeTrue();
    }

    [Fact]
    public async Task StopWatchingAsync_WhenNotWatching_DoesNotThrow()
    {
        // Arrange
        _fileWatcherService.IsWatching.Should().BeFalse();

        // Act & Assert
        await _fileWatcherService.StopWatchingAsync();
        _fileWatcherService.IsWatching.Should().BeFalse();
    }

    public void Dispose()
    {
        _fileWatcherService?.Dispose();
        
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
