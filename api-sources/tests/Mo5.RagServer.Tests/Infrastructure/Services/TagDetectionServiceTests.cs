using FluentAssertions;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Infrastructure.Services;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class TagDetectionServiceTests
{
    private readonly TagDetectionService _tagDetectionService = new();
    private readonly List<Tag> _availableTags;

    public TagDetectionServiceTests()
    {
        _availableTags = new List<Tag>
        {
            new() { Id = Guid.NewGuid(), Name = "C", Category = "language", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Assembly", Category = "language", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Basic", Category = "language", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "text-mode", Category = "mode", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "graphics-mode", Category = "mode", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "hardware", Category = "topic", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "tools", Category = "topic", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "examples", Category = "topic", IsActive = true }
        };
    }

    [Fact]
    public async Task DetectTagsAsync_WithCContent_DetectsCTag()
    {
        // Arrange
        var fileName = "c-programming-guide.md";
        var content = @"# C Programming Guide

This guide covers C programming for the MO5.

```c
#include <stdio.h>

int main() {
    printf(""Hello, MO5!"");
    return 0;
}
```

Use gcc to compile your C programs.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "C");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "examples");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "tools");
        
        var cTag = detectedTags.First(dt => dt.Tag.Name == "C");
        cTag.Confidence.Should().BeGreaterThan(0.8f);
    }

    [Fact]
    public async Task DetectTagsAsync_WithAssemblyContent_DetectsAssemblyTag()
    {
        // Arrange
        var fileName = "assembly-graphics-mode.md";
        var content = @"# Assembly Graphics Mode

Programming graphics in 6809 assembly for the MO5.

```assembly
    lda #$FF
    sta $A000
    jmp loop
```

The 6809 processor supports various addressing modes.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "Assembly");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "graphics-mode");
        
        var assemblyTag = detectedTags.First(dt => dt.Tag.Name == "Assembly");
        assemblyTag.Confidence.Should().BeGreaterThan(0.8f);
    }

    [Fact]
    public async Task DetectTagsAsync_WithTextModeContent_DetectsTextModeTag()
    {
        // Arrange
        var fileName = "text-display.md";
        var content = @"# Text Mode Display

Working with text mode on the MO5.

The console supports 40x25 character display.
Use cursor positioning for text layout.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "text-mode");
        
        var textModeTag = detectedTags.First(dt => dt.Tag.Name == "text-mode");
        textModeTag.Confidence.Should().BeGreaterThan(0.6f);
    }

    [Fact]
    public async Task DetectTagsAsync_WithHardwareContent_DetectsHardwareTag()
    {
        // Arrange
        var fileName = "mo5-specifications.md";
        var content = @"# MO5 Hardware Specifications

Memory map and hardware registers.

ROM: $C000-$FFFF
RAM: $0000-$9FFF
PIA: $A7C0-$A7C3

Interrupt vectors are located at $FFF8-$FFFF.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "hardware");
        
        var hardwareTag = detectedTags.First(dt => dt.Tag.Name == "hardware");
        hardwareTag.Confidence.Should().BeGreaterThan(0.7f);
    }

    [Fact]
    public async Task DetectTagsAsync_WithBasicContent_DetectsBasicTag()
    {
        // Arrange
        var fileName = "basic-programming.md";
        var content = @"# BASIC Programming

Programming in BASIC on the MO5.

10 PRINT ""Hello, World!""
20 FOR I = 1 TO 10
30 PRINT I
40 NEXT I
50 END

Use POKE and PEEK for memory access.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "Basic");
        
        var basicTag = detectedTags.First(dt => dt.Tag.Name == "Basic");
        basicTag.Confidence.Should().BeGreaterThan(0.8f);
    }

    [Fact]
    public async Task DetectTagsAsync_WithMultipleLanguages_DetectsMultipleTags()
    {
        // Arrange
        var fileName = "mixed-programming.md";
        var content = @"# Mixed Programming Examples

Examples in C and Assembly.

C example:
```c
#include <stdio.h>
int main() { return 0; }
```

Assembly example:
```asm
lda #$00
sta $A000
```

These examples show graphics mode programming.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        detectedTags.Should().NotBeEmpty();
        detectedTags.Should().Contain(dt => dt.Tag.Name == "C");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "Assembly");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "graphics-mode");
        detectedTags.Should().Contain(dt => dt.Tag.Name == "examples");
    }

    [Fact]
    public async Task DetectTagsAsync_WithInactiveTag_DoesNotDetectInactiveTag()
    {
        // Arrange
        var inactiveTags = _availableTags.ToList();
        inactiveTags.First(t => t.Name == "C").IsActive = false;

        var fileName = "c-programming.md";
        var content = "#include <stdio.h>\nint main() { return 0; }";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, inactiveTags);

        // Assert
        detectedTags.Should().NotContain(dt => dt.Tag.Name == "C");
    }

    [Fact]
    public async Task DetectTagsAsync_WithLowConfidenceContent_FiltersLowConfidenceTags()
    {
        // Arrange
        var fileName = "generic-document.md";
        var content = "This is a generic document with no specific programming content.";

        // Act
        var detectedTags = await _tagDetectionService.DetectTagsAsync(fileName, content, _availableTags);

        // Assert
        // Should not detect any tags with high confidence for generic content
        detectedTags.Should().BeEmpty();
    }
}
