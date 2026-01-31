using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mo5.RagServer.Core.Interfaces;
using Mo5.RagServer.Core.Models;
using Mo5.RagServer.Tests.Integration;
using Moq;
using Xunit;

namespace Mo5.RagServer.Tests.Api;

/// <summary>
/// API tests for SearchController
/// NOTE: These tests require PostgreSQL with pgvector extension and are skipped by default.
/// </summary>
[Trait("Category", "RequiresPostgreSQL")]
public class SearchControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SearchControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the document service with a mock for testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockDocumentService = new Mock<IDocumentService>();

                // Setup mock responses
                mockDocumentService.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SearchRequest request, CancellationToken _) => new SearchResponse
                    {
                        Query = request.Query,
                        Results = new List<SearchResult>
                        {
                            new()
                            {
                                ChunkId = Guid.NewGuid(),
                                Content = "Sample content about C programming",
                                SimilarityScore = 0.85f,
                                Document = new DocumentMetadata
                                {
                                    DocumentId = Guid.NewGuid(),
                                    FileName = "c-programming.md",
                                    Title = "C Programming Guide",
                                    FilePath = "c-programming.md",
                                    LastModified = DateTime.UtcNow,
                                    Tags = new List<string> { "C", "examples" }
                                },
                                Position = new ChunkPosition
                                {
                                    ChunkIndex = 0,
                                    StartPosition = 0,
                                    EndPosition = 37,
                                    SectionHeading = "Introduction"
                                }
                            }
                        },
                        TotalResults = 1,
                        ExecutionTimeMs = 150,
                        Filters = new SearchFilters
                        {
                            Tags = request.Tags,
                            MinSimilarityScore = request.MinSimilarityScore,
                            MaxResults = request.MaxResults
                        }
                    });

                services.AddSingleton(mockDocumentService.Object);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Search_Post_WithValidRequest_ReturnsResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "C programming",
            MaxResults = 10,
            MinSimilarityScore = 0.7f,
            Tags = new List<string>(),
            IncludeMetadata = true,
            IncludeContext = false
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Query.Should().Be("C programming");
        searchResponse.Results.Should().HaveCount(1);
        searchResponse.Results.First().SimilarityScore.Should().Be(0.85f);
    }

    [Fact]
    public async Task Search_Get_WithValidQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/search?q=C programming&maxResults=5&minScore=0.8");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Query.Should().Be("C programming");
        searchResponse.Results.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_Post_WithEmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            MaxResults = 10,
            MinSimilarityScore = 0.7f
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_Get_WithoutQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_Get_WithTagFilter_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/search?q=programming&tags=C,examples");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        searchResponse.Should().NotBeNull();
        searchResponse!.Filters.Tags.Should().Contain("C");
        searchResponse.Filters.Tags.Should().Contain("examples");
    }

    [Fact]
    public async Task Suggestions_WithValidPartial_ReturnsSuggestions()
    {
        // Act
        var response = await _client.GetAsync("/api/search/suggestions?partial=6809&limit=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var suggestions = JsonSerializer.Deserialize<List<string>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        suggestions.Should().NotBeNull();
        suggestions.Should().Contain(s => s.Contains("6809", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Suggestions_WithShortPartial_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/search/suggestions?partial=a");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var suggestions = JsonSerializer.Deserialize<List<string>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        suggestions.Should().NotBeNull();
        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be("Healthy");
    }
}
