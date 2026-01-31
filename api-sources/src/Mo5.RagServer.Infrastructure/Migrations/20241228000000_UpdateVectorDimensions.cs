using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mo5.RagServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVectorDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing vector index
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_DocumentChunks_Embedding\";");
            
            // Drop existing embedding column
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "DocumentChunks");

            // Add new embedding column with 384 dimensions (for local embeddings)
            migrationBuilder.AddColumn<Pgvector.Vector>(
                name: "Embedding",
                table: "DocumentChunks",
                type: "vector(384)",
                nullable: false);

            // Recreate vector index for cosine similarity
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_DocumentChunks_Embedding"" 
                ON ""DocumentChunks"" 
                USING ivfflat (""Embedding"" vector_cosine_ops) 
                WITH (lists = 100);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new vector index
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_DocumentChunks_Embedding\";");
            
            // Drop the 384-dimension embedding column
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "DocumentChunks");

            // Add back the original 1536-dimension embedding column
            migrationBuilder.AddColumn<Pgvector.Vector>(
                name: "Embedding",
                table: "DocumentChunks",
                type: "vector(1536)",
                nullable: false);

            // Recreate the original vector index
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_DocumentChunks_Embedding"" 
                ON ""DocumentChunks"" 
                USING ivfflat (""Embedding"" vector_cosine_ops) 
                WITH (lists = 100);
            ");
        }
    }
}
