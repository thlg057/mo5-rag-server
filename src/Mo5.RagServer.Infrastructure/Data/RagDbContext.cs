using Microsoft.EntityFrameworkCore;
using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Infrastructure.Data;

public class RagDbContext : DbContext
{
    public RagDbContext(DbContextOptions<RagDbContext> options) : base(options)
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

            // Indexes
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.ContentHash);
            entity.HasIndex(e => e.IsActive);
        });

        // DocumentChunk configuration
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentId).IsRequired();
            entity.Property(e => e.ChunkIndex).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            // Vector size for local embeddings (384 dimensions)
            entity.Property(e => e.Embedding).IsRequired().HasColumnType("vector(384)");
            entity.Property(e => e.StartPosition).IsRequired();
            entity.Property(e => e.EndPosition).IsRequired();
            entity.Property(e => e.Length).IsRequired();
            entity.Property(e => e.TokenCount).IsRequired();
            entity.Property(e => e.SectionHeading).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Relationships
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Chunks)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => new { e.DocumentId, e.ChunkIndex }).IsUnique();
            entity.HasIndex(e => e.Embedding).HasMethod("ivfflat").HasOperators("vector_cosine_ops");
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

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // DocumentTag configuration (many-to-many)
        modelBuilder.Entity<DocumentTag>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.TagId });
            entity.Property(e => e.AssignedAt).IsRequired();
            entity.Property(e => e.AssignmentSource).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Confidence).IsRequired();

            // Relationships
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.DocumentTags)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.DocumentTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.AssignmentSource);
            entity.HasIndex(e => e.Confidence);
        });
    }
}
