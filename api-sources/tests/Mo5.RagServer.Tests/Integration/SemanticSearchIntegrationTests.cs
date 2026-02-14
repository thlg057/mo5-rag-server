using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Infrastructure.Data;
using Mo5.RagServer.Infrastructure.Services;
using Xunit;

namespace Mo5.RagServer.Tests.Integration;

/// <summary>
/// Integration tests for semantic search functionality
/// Tests the fixes for TF-IDF vocabulary initialization and embedding regeneration
/// </summary>
public class SemanticSearchIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SemanticSearchIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public void TfIdfService_ShouldBeSingleton_AcrossRequests()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        // Act
        var service1 = scope1.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IEmbeddingService>();

        // Assert
        service1.Should().BeSameAs(service2, "TF-IDF service should be registered as Singleton");
    }

    [Fact]
    public async Task TfIdfVocabulary_ShouldBeInitializedWithChunks_NotDocuments()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();

        // Act
        var totalChunks = await dbContext.DocumentChunks.CountAsync();
        var totalDocuments = await dbContext.Documents.CountAsync();

        // Assert
        totalChunks.Should().BeGreaterThan(0, "should have indexed chunks");
        totalDocuments.Should().BeGreaterThan(0, "should have indexed documents");
        totalChunks.Should().Be(totalDocuments, "with the new ingestion rule: 1 .md file = 1 chunk");

        // If it's SimpleTfIdfEmbeddingService, verify it's initialized
        if (embeddingService is SimpleTfIdfEmbeddingService tfIdfService)
        {
            // Generate a test embedding to verify vocabulary is initialized
            var testEmbedding = await tfIdfService.GenerateEmbeddingAsync("test query", CancellationToken.None);
            testEmbedding.Should().NotBeNull();
            testEmbedding.ToArray().Length.Should().Be(384, "embedding should have correct dimensions");
        }
    }

    [Fact]
    public async Task AllChunks_ShouldHaveValidEmbeddings_AfterInitialization()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();

        // Act
        var chunks = await dbContext.DocumentChunks.ToListAsync();

        // Assert
        chunks.Should().NotBeEmpty("should have indexed chunks");
        chunks.Should().OnlyContain(c => c.Embedding != null, "all chunks should have embeddings");
        chunks.Should().OnlyContain(c => c.Embedding!.ToArray().Length == 384, "all embeddings should have 384 dimensions");
    }

    [Fact]
    public async Task SemanticSearch_ShouldReturnResults_WithHighSimilarityScores()
    {
        // Arrange
        var searchRequest = new
        {
            query = "graphics mode",
            maxResults = 5,
            minSimilarityScore = 0.1
        };

        var searchJson = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(searchJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        searchData.GetProperty("totalResults").GetInt32().Should().BeGreaterThan(0, "should return results");
        
        var results = searchData.GetProperty("results").EnumerateArray().ToList();
        results.Should().NotBeEmpty("should have search results");

        // Verify similarity scores are reasonable (not all zeros)
        var topResult = results.First();
        var topScore = topResult.GetProperty("similarityScore").GetDouble();
        topScore.Should().BeGreaterThan(0.1, "top result should have a non-trivial similarity score");
    }

    [Theory]
    [InlineData("graphics mode", 0.1, "should find graphics mode content")]
    [InlineData("address calculation", 0.1, "should find address calculation content")]
    [InlineData("Thomson MO5", 0.1, "should find Thomson MO5 references")]
    public async Task SemanticSearch_WithSpecificQueries_ReturnsRelevantResults(
        string query, 
        double expectedMinScore, 
        string because)
    {
        // Arrange
        var searchRequest = new
        {
            query = query,
            maxResults = 3,
            minSimilarityScore = 0.1
        };

        var searchJson = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(searchJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var totalResults = searchData.GetProperty("totalResults").GetInt32();
        totalResults.Should().BeGreaterThan(0, because);

        if (totalResults > 0)
        {
            var results = searchData.GetProperty("results").EnumerateArray().ToList();
            var topScore = results.First().GetProperty("similarityScore").GetDouble();
            topScore.Should().BeGreaterThanOrEqualTo(expectedMinScore, 
                $"top result for '{query}' should have similarity >= {expectedMinScore}");
        }
    }
}

