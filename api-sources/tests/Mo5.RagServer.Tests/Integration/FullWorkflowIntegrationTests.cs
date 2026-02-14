using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mo5.RagServer.Core.Models;
using Xunit;

namespace Mo5.RagServer.Tests.Integration;

/// <summary>
/// Integration tests for the complete RAG workflow
/// Configured to run with EF InMemory + local TF-IDF embeddings via CustomWebApplicationFactory.
/// </summary>
public class FullWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FullWorkflowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task CompleteWorkflow_IndexAndSearch_WorksEndToEnd()
    {
        // Step 1: Check initial status
        var statusResponse = await _client.GetAsync("/api/index/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Trigger indexing
        var indexResponse = await _client.PostAsync("/api/index/all", null);
        indexResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Indexing endpoint is synchronous, but keep a small delay for stability
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        // Step 3: Check status after indexing
        var statusAfterResponse = await _client.GetAsync("/api/index/status");
        statusAfterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusContent = await statusAfterResponse.Content.ReadAsStringAsync();
        var statusData = JsonSerializer.Deserialize<JsonElement>(statusContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Should have some documents indexed
        statusData.GetProperty("totalDocuments").GetInt32().Should().BeGreaterThan(0);

        // Step 4: Get documents list
        var documentsResponse = await _client.GetAsync("/api/documents");
        documentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Perform search
        var searchRequest = new
        {
            query = "programming",
            maxResults = 5,
            minSimilarityScore = 0.1,
            includeMetadata = true
        };

        var searchJson = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var searchContent = new StringContent(searchJson, Encoding.UTF8, "application/json");

        var searchResponse = await _client.PostAsync("/api/search", searchContent);
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResponseContent = await searchResponse.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(searchResponseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Should return search results
        searchData.GetProperty("query").GetString().Should().Be("programming");
        searchData.GetProperty("totalResults").GetInt32().Should().BeGreaterThanOrEqualTo(0);

        // Step 6: Test GET search endpoint
        var getSearchResponse = await _client.GetAsync("/api/search?q=programming&maxResults=3");
        getSearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Test suggestions
        var suggestionsResponse = await _client.GetAsync("/api/search/suggestions?partial=prog&limit=5");
        suggestionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 8: Check ingestion stats
        var ingestionStatsResponse = await _client.GetAsync("/api/ingestion/stats");
        ingestionStatsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 9: Check file watcher status
        var watcherStatusResponse = await _client.GetAsync("/api/ingestion/watcher/status");
        watcherStatusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var watcherContent = await watcherStatusResponse.Content.ReadAsStringAsync();
        var watcherData = JsonSerializer.Deserialize<JsonElement>(watcherContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // In tests we disable hosted services (file watcher) to keep the suite deterministic.
        watcherData.GetProperty("isWatching").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task SearchEndpoints_WithVariousParameters_ReturnValidResponses()
    {
        // Test empty query
        var emptyQueryResponse = await _client.GetAsync("/api/search?q=");
        emptyQueryResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test with tags filter
        var taggedSearchResponse = await _client.GetAsync("/api/search?q=test&tags=C,Assembly");
        taggedSearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test with various parameters
        var parameterizedResponse = await _client.GetAsync("/api/search?q=programming&maxResults=2&minScore=0.8");
        parameterizedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test POST search with complex request
        var complexSearchRequest = new
        {
            query = "6809 assembly",
            maxResults = 10,
            minSimilarityScore = 0.5,
            tags = new[] { "Assembly" },
            includeMetadata = true,
            includeContext = true
        };

        var complexJson = JsonSerializer.Serialize(complexSearchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var complexContent = new StringContent(complexJson, Encoding.UTF8, "application/json");

        var complexResponse = await _client.PostAsync("/api/search", complexContent);
        complexResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DocumentsEndpoints_ReturnValidData()
    {
        // Get all documents
        var allDocsResponse = await _client.GetAsync("/api/documents");
        allDocsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get documents with tag filter
        var filteredDocsResponse = await _client.GetAsync("/api/documents?tags=C");
        filteredDocsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get available tags
        var tagsResponse = await _client.GetAsync("/api/documents/tags");
        tagsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tagsContent = await tagsResponse.Content.ReadAsStringAsync();
        var tagsData = JsonSerializer.Deserialize<JsonElement[]>(tagsContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        tagsData.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IngestionEndpoints_ReturnValidStats()
    {
        // Get ingestion statistics
        var statsResponse = await _client.GetAsync("/api/ingestion/stats");
        statsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var statsData = JsonSerializer.Deserialize<JsonElement>(statsContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Should have basic stats structure
        statsData.TryGetProperty("totalFilesProcessed", out _).Should().BeTrue();
        statsData.TryGetProperty("successfulIndexings", out _).Should().BeTrue();
        statsData.TryGetProperty("isWatching", out _).Should().BeTrue();

        // Get recent activities
        var activitiesResponse = await _client.GetAsync("/api/ingestion/activities?limit=10");
        activitiesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Reset stats (should work)
        var resetResponse = await _client.PostAsync("/api/ingestion/stats/reset", null);
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ErrorHandling_ReturnsAppropriateStatusCodes()
    {
        // Test invalid search request
        var invalidSearchJson = "{ invalid json }";
        var invalidContent = new StringContent(invalidSearchJson, Encoding.UTF8, "application/json");
        var invalidResponse = await _client.PostAsync("/api/search", invalidContent);
        invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Test non-existent document
        var nonExistentDocResponse = await _client.GetAsync($"/api/documents/{Guid.NewGuid()}");
        nonExistentDocResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Test invalid suggestions request
        var invalidSuggestionsResponse = await _client.GetAsync("/api/search/suggestions?partial=a"); // Too short
        invalidSuggestionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var suggestionsContent = await invalidSuggestionsResponse.Content.ReadAsStringAsync();
        var suggestions = JsonSerializer.Deserialize<string[]>(suggestionsContent);
        suggestions.Should().BeEmpty(); // Should return empty for short partials
    }

    [Theory]
    [InlineData("/api/search?q=C programming")]
    [InlineData("/api/search?q=assembly language")]
    [InlineData("/api/search?q=6809 processor")]
    [InlineData("/api/search?q=graphics mode")]
    public async Task SearchQueries_ForMO5Content_ReturnResults(string searchUrl)
    {
        // Act
        var response = await _client.GetAsync(searchUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Should have valid search response structure
        searchData.TryGetProperty("query", out _).Should().BeTrue();
        searchData.TryGetProperty("results", out _).Should().BeTrue();
        searchData.TryGetProperty("totalResults", out _).Should().BeTrue();
        searchData.TryGetProperty("executionTimeMs", out _).Should().BeTrue();
    }
}
