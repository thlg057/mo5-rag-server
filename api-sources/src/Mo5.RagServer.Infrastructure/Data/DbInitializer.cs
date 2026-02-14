using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(RagDbContext context, ILogger logger)
    {
        try
        {
            // Ensure database is created and migrations are applied.
            // In tests we often use the EF InMemory provider which doesn't support relational migrations.
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created successfully (non-relational provider)");
            }

            // Seed default tags if they don't exist
            await SeedDefaultTagsAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    private static async Task SeedDefaultTagsAsync(RagDbContext context, ILogger logger)
    {
        if (await context.Tags.AnyAsync())
        {
            logger.LogInformation("Tags already exist, skipping seeding");
            return;
        }

        var defaultTags = new[]
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "C",
                Description = "C programming language content",
                Category = "language",
                Color = "#00599C",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Assembly",
                Description = "Assembly language content (6809)",
                Category = "language",
                Color = "#6E4C13",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Basic",
                Description = "BASIC programming language content",
                Category = "language",
                Color = "#FF6B35",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "text-mode",
                Description = "Text mode programming and display",
                Category = "mode",
                Color = "#2563EB",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "graphics-mode",
                Description = "Graphics mode programming and display",
                Category = "mode",
                Color = "#DC2626",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "hardware",
                Description = "Hardware specifications and registers",
                Category = "topic",
                Color = "#059669",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "tools",
                Description = "Development tools and compilation",
                Category = "topic",
                Color = "#7C3AED",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "examples",
                Description = "Code examples and samples",
                Category = "topic",
                Color = "#EA580C",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        context.Tags.AddRange(defaultTags);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} default tags", defaultTags.Length);
    }
}
