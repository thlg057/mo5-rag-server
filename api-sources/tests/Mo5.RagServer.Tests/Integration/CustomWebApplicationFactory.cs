using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Infrastructure.Data;
using Mo5.RagServer.Infrastructure.Services;

namespace Mo5.RagServer.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Replaces PostgreSQL with in-memory database
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestApiKey = "test-api-key";
    private readonly string _knowledgeBasePath;
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        _knowledgeBasePath = CreateTestKnowledgeBase();
        _databaseName = $"InMemoryTestDb-{Guid.NewGuid():N}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RagSettings:KnowledgeBasePath"] = _knowledgeBasePath,
                ["EmbeddingService:Provider"] = "TfIdf",
                ["ApiKeySettings:Key"] = TestApiKey,

                // Provide a dummy connection string so parts of the app that read it don't crash.
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=mo5_rag_test;Username=test;Password=test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace PostgreSQL DbContext with EF InMemory
            services.RemoveAll(typeof(DbContextOptions<RagDbContext>));
            services.RemoveAll(typeof(RagDbContext));

            services.AddDbContext<RagDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Disable background services that might cause issues in tests
            services.RemoveAll(typeof(IHostedService));

            // Ensure embeddings don't require external HTTP services in tests
            services.RemoveAll(typeof(IEmbeddingService));
            services.AddSingleton<IEmbeddingService, SimpleTfIdfEmbeddingService>();

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to initialize TF-IDF vocab and index the test knowledge base
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;

                var db = scopedServices.GetRequiredService<RagDbContext>();
                db.Database.EnsureCreated();

                // Initialize TF-IDF with the whole corpus BEFORE indexing so embeddings are stable.
                var embeddingService = scopedServices.GetRequiredService<IEmbeddingService>();
                if (embeddingService is SimpleTfIdfEmbeddingService tfidf)
                {
                    var corpus = Directory
                        .GetFiles(_knowledgeBasePath, "*.md", SearchOption.AllDirectories)
                        .Select(File.ReadAllText)
                        .ToList();

                    tfidf.InitializeWithCorpusAsync(corpus).GetAwaiter().GetResult();
                }

                var documentService = scopedServices.GetRequiredService<IDocumentService>();
                documentService.IndexDocumentsAsync(_knowledgeBasePath, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        // Add API key for endpoints protected by the ApiKey auth scheme.
        client.DefaultRequestHeaders.Remove("X-Api-Key");
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
    }

    private static string CreateTestKnowledgeBase()
    {
        var root = Path.Combine(Path.GetTempPath(), "mo5-rag-tests", "knowledge", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        // The folder names are used as tags.
        WriteMd(root, Path.Combine("Thomson", "MO5", "graphics-mode.md"), "# Graphics mode\n\nThomson MO5 graphics mode and display settings.");
        WriteMd(root, Path.Combine("Assembly", "6809", "address-calculation.md"), "# Address calculation\n\n6809 processor address calculation examples in assembly.");
        WriteMd(root, Path.Combine("C", "c-programming.md"), "# C programming\n\nC programming examples for the Thomson MO5.");
        WriteMd(root, Path.Combine("Manuals", "overview.md"), "# Overview\n\nMO5 overview document.");

        return root;
    }

    private static void WriteMd(string root, string relativePath, string content)
    {
        var fullPath = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }
}