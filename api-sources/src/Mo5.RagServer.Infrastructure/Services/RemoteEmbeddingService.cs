using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Pgvector;

namespace Mo5.RagServer.Infrastructure.Services;

public class RemoteEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemoteEmbeddingService> _logger;
    private readonly string _endpoint;

    public int EmbeddingDimension => 384;
    public int MaxTokens => 512;

    public RemoteEmbeddingService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<RemoteEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Récupère http://embedding-api:5000/embed depuis le docker-compose
        _endpoint = configuration.GetValue<string>("EmbeddingService:Endpoint") 
                    ?? throw new ArgumentNullException("EmbeddingService:Endpoint is not configured");
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var results = await GenerateEmbeddingsAsync(new[] { text }, ct);
        return results.FirstOrDefault() ?? throw new InvalidOperationException("Failed to generate embedding");
    }

    public async Task<Vector[]> GenerateEmbeddingsAsync(string[] texts, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_endpoint, new { texts }, ct);
            response.EnsureSuccessStatusCode();

            var embeddings = await response.Content.ReadFromJsonAsync<float[][]>(cancellationToken: ct);
            return embeddings?.Select(e => new Vector(e)).ToArray() ?? Array.Empty<Vector>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling remote embedding service at {Endpoint}", _endpoint);
            throw;
        }
    }
}