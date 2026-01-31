using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Mo5.RagServer.Tests.Integration;
using Xunit;

namespace Mo5.RagServer.Tests.Performance;

/// <summary>
/// Performance tests for search operations
/// NOTE: These tests require PostgreSQL with pgvector extension and are skipped by default.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class SearchPerformanceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SearchPerformanceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SearchPerformance_SingleQuery_CompletesWithinReasonableTime()
    {
        // Arrange
        var searchRequest = new
        {
            query = "C programming on MO5",
            maxResults = 10,
            minSimilarityScore = 0.7
        };

        var json = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/search", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Check that execution time is reported
        var executionTime = searchData.GetProperty("executionTimeMs").GetInt64();
        executionTime.Should().BeGreaterThan(0);
        executionTime.Should().BeLessThan(3000); // API should report < 3 seconds
    }

    [Fact]
    public async Task SearchPerformance_ConcurrentQueries_HandlesLoadWell()
    {
        // Arrange
        var queries = new[]
        {
            "C programming",
            "assembly language",
            "6809 processor",
            "graphics mode",
            "text mode",
            "memory management",
            "interrupts",
            "sound programming"
        };

        var tasks = new List<Task<(TimeSpan Duration, bool Success)>>();

        // Act
        foreach (var query in queries)
        {
            tasks.Add(PerformSearchAsync(query));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulQueries = results.Count(r => r.Success);
        var averageDuration = results.Where(r => r.Success).Average(r => r.Duration.TotalMilliseconds);
        var maxDuration = results.Where(r => r.Success).Max(r => r.Duration.TotalMilliseconds);

        successfulQueries.Should().Be(queries.Length); // All queries should succeed
        averageDuration.Should().BeLessThan(3000); // Average should be < 3 seconds
        maxDuration.Should().BeLessThan(10000); // Max should be < 10 seconds
    }

    [Fact]
    public async Task IndexingPerformance_FullReindex_CompletesWithinReasonableTime()
    {
        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/index/all", null);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Initial response should be quick (async operation)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);

        // Wait for indexing to complete and check status
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var statusResponse = await _client.GetAsync("/api/index/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task SearchPerformance_VaryingResultLimits_ScalesAppropriately(int maxResults)
    {
        // Arrange
        var searchRequest = new
        {
            query = "programming",
            maxResults = maxResults,
            minSimilarityScore = 0.5
        };

        var json = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync("/api/search", content);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var searchData = JsonSerializer.Deserialize<JsonElement>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var actualResults = searchData.GetProperty("results").GetArrayLength();
        actualResults.Should().BeLessOrEqualTo(maxResults);

        // Performance should not degrade significantly with more results
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task MemoryUsage_MultipleSearches_DoesNotLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var searchRequest = new
        {
            query = "test query",
            maxResults = 10
        };

        var json = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Act - Perform many searches
        for (int i = 0; i < 50; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/search", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Read and dispose response
            await response.Content.ReadAsStringAsync();
            response.Dispose();
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);

        // Assert - Memory should not have grown excessively
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
        
        // Allow some memory increase but not excessive
        memoryIncreaseMB.Should().BeLessThan(100); // Less than 100MB increase
    }

    private async Task<(TimeSpan Duration, bool Success)> PerformSearchAsync(string query)
    {
        try
        {
            var searchRequest = new
            {
                query = query,
                maxResults = 5,
                minSimilarityScore = 0.5
            };

            var json = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsync("/api/search", content);
            stopwatch.Stop();

            var success = response.StatusCode == HttpStatusCode.OK;
            return (stopwatch.Elapsed, success);
        }
        catch
        {
            return (TimeSpan.Zero, false);
        }
    }
}
