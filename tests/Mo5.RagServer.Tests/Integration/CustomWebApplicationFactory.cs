using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Replaces PostgreSQL with in-memory database
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TestRagDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the RagDbContext registration
            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TestRagDbContext));

            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Add in-memory database for testing with custom DbContext
            services.AddDbContext<TestRagDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            // Disable background services that might cause issues in tests
            services.RemoveAll(typeof(IHostedService));

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TestRagDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            }
        });

        builder.UseEnvironment("Testing");
    }
}

/// <summary>
/// Test-specific DbContext that ignores Vector properties for in-memory database
/// </summary>
public class TestRagDbContext : DbContext
{
    public TestRagDbContext(DbContextOptions<TestRagDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.LastModified).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.ContentHash);
            entity.HasIndex(e => e.IsActive);
        });

        // DocumentChunk configuration - IGNORE Embedding for in-memory database
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentId).IsRequired();
            entity.Property(e => e.ChunkIndex).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            // IGNORE Embedding property for in-memory database
            entity.Ignore(e => e.Embedding);
            entity.Property(e => e.StartPosition).IsRequired();
            entity.Property(e => e.EndPosition).IsRequired();
            entity.Property(e => e.Length).IsRequired();
            entity.Property(e => e.TokenCount).IsRequired();
            entity.Property(e => e.SectionHeading).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Chunks)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => new { e.DocumentId, e.ChunkIndex }).IsUnique();
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // DocumentTag configuration (many-to-many)
        modelBuilder.Entity<DocumentTag>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.TagId });

            entity.HasOne(e => e.Document)
                  .WithMany(d => d.DocumentTags)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.DocumentTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

