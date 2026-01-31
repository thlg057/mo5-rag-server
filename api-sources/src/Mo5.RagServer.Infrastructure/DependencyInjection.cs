using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Services;
using Mo5.RagServer.Infrastructure.Data;
using Mo5.RagServer.Infrastructure.Services;

namespace Mo5.RagServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<RagDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.UseVector()));

        // Add embedding service based on configuration
        var embeddingProvider = configuration.GetValue<string>("EmbeddingService:Provider", "TfIdf") ?? "TfIdf";
        services.AddHttpClient<IEmbeddingService, RemoteEmbeddingService>(client =>
        {
            // L'URL doit correspondre au nom du service dans votre docker-compose
            client.BaseAddress = new Uri("http://embedding-api:5000"); 
        });
        // switch (embeddingProvider.ToLowerInvariant())
        // {
        //     case "remote":
        //         services.AddHttpClient<IEmbeddingService, RemoteEmbeddingService>(client =>
        //         {
        //             // L'URL doit correspondre au nom du service dans votre docker-compose
        //             client.BaseAddress = new Uri("http://embedding-api:5000"); 
        //         });
        //         break;
        //     case "local":
        //         services.AddScoped<IEmbeddingService, LocalEmbeddingService>();
        //         break;
        //     case "tfidf":
        //     default:
        //         // Use Singleton for TF-IDF to preserve vocabulary across requests
        //         services.AddSingleton<IEmbeddingService, SimpleTfIdfEmbeddingService>();
        //         break;
        // }
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ITextChunker, MarkdownTextChunker>();
        services.AddScoped<ITagDetectionService, TagDetectionService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IIngestionStatsService, IngestionStatsService>();

        // Add hosted services
        // services.AddHostedService<DatabaseInitializationService>();
        services.AddHostedService<DocumentIngestionService>();

        return services;
    }
}
