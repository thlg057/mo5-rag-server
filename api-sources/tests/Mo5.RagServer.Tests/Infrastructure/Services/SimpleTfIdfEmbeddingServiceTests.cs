using Microsoft.Extensions.Logging;
using Mo5.RagServer.Infrastructure.Services;
using Moq;
using Xunit;

namespace Mo5.RagServer.Tests.Infrastructure.Services;

public class SimpleTfIdfEmbeddingServiceTests
{
    private readonly Mock<ILogger<SimpleTfIdfEmbeddingService>> _mockLogger;
    private readonly SimpleTfIdfEmbeddingService _service;

    public SimpleTfIdfEmbeddingServiceTests()
    {
        _mockLogger = new Mock<ILogger<SimpleTfIdfEmbeddingService>>();
        _service = new SimpleTfIdfEmbeddingService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithValidText_ShouldReturnVector()
    {
        // Arrange
        var text = "Programmation en assembleur 6809 pour Thomson MO5";

        // Act
        var result = await _service.GenerateEmbeddingAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(384, result.ToArray().Length); // Vérifier la taille du vecteur
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithNullText_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateEmbeddingAsync(null!));
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithEmptyText_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateEmbeddingAsync(""));
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_WithMultipleTexts_ShouldReturnCorrectCount()
    {
        // Arrange
        var texts = new List<string>
        {
            "Programmation en C pour MO5",
            "Assembleur 6809 et registres",
            "Mode graphique et affichage",
            "Gestion des interruptions"
        };

        // Act
        var results = await _service.GenerateEmbeddingsAsync(texts.ToArray());

        // Assert
        Assert.Equal(texts.Count, results.Length);
        Assert.All(results, vector => Assert.Equal(384, vector.ToArray().Length));
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GenerateEmbeddingsAsync(Array.Empty<string>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_SimilarTexts_ShouldProduceSimilarVectors()
    {
        // Arrange - Initialize vocabulary with a corpus
        var corpus = new List<string>
        {
            "Programmation en assembleur 6809 pour Thomson MO5",
            "Développement assembleur pour processeur 6809",
            "Le processeur 6809 est utilisé dans le Thomson MO5",
            "Cuisine française et recettes traditionnelles",
            "Recettes de cuisine et gastronomie"
        };
        await _service.InitializeWithCorpusAsync(corpus);

        var text1 = "Programmation en assembleur 6809";
        var text2 = "Développement assembleur pour processeur 6809";
        var text3 = "Cuisine française et recettes traditionnelles"; // Texte très différent

        // Act
        var vector1 = await _service.GenerateEmbeddingAsync(text1);
        var vector2 = await _service.GenerateEmbeddingAsync(text2);
        var vector3 = await _service.GenerateEmbeddingAsync(text3);

        // Assert
        var similarity12 = CosineSimilarity(vector1.ToArray(), vector2.ToArray());
        var similarity13 = CosineSimilarity(vector1.ToArray(), vector3.ToArray());

        // Les textes similaires devraient avoir une similarité plus élevée
        Assert.True(similarity12 > similarity13,
            $"Similarity between similar texts ({similarity12:F3}) should be higher than dissimilar texts ({similarity13:F3})");
    }

    [Fact]
    public async Task InitializeWithCorpusAsync_ShouldBuildVocabulary()
    {
        // Arrange
        var corpus = new List<string>
        {
            "Le processeur 6809 est utilisé dans le Thomson MO5",
            "La programmation en assembleur permet un contrôle précis",
            "Le mode graphique offre 320x200 pixels en 16 couleurs",
            "Les interruptions permettent la gestion des événements"
        };

        // Act
        await _service.InitializeWithCorpusAsync(corpus);
        var stats = _service.GetVocabularyStats();

        // Assert
        Assert.True((bool)stats["IsInitialized"]);
        Assert.True((int)stats["VocabularySize"] > 0);
        Assert.Equal(384, (int)stats["VectorSize"]);
    }

    [Fact]
    public void GetVocabularyStats_ShouldReturnValidStats()
    {
        // Act
        var stats = _service.GetVocabularyStats();

        // Assert
        Assert.Contains("VocabularySize", stats);
        Assert.Contains("VectorSize", stats);
        Assert.Contains("IsInitialized", stats);
        Assert.Contains("TopTerms", stats);
        Assert.Equal(384, stats["VectorSize"]);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var text = "Programmation C: int main() { printf(\"Hello MO5!\"); return 0; }";

        // Act
        var result = await _service.GenerateEmbeddingAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(384, result.ToArray().Length);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithFrenchText_ShouldFilterStopWords()
    {
        // Arrange - Initialize vocabulary with a corpus
        var corpus = new List<string>
        {
            "Le processeur de la machine est un 6809",
            "Le processeur 6809 fonctionne très bien",
            "La machine Thomson MO5 utilise le processeur 6809",
            "Le 6809 est un excellent processeur"
        };
        await _service.InitializeWithCorpusAsync(corpus);

        var textWithStopWords = "Le processeur de la machine est un 6809 et il fonctionne très bien";
        var textWithoutStopWords = "processeur machine 6809 fonctionne bien";

        // Act
        var vector1 = await _service.GenerateEmbeddingAsync(textWithStopWords);
        var vector2 = await _service.GenerateEmbeddingAsync(textWithoutStopWords);

        // Assert
        // Les vecteurs devraient être similaires car les mots vides sont filtrés
        var similarity = CosineSimilarity(vector1.ToArray(), vector2.ToArray());
        Assert.True(similarity > 0.5, $"Similarity should be high ({similarity:F3}) after stop word filtering");
    }

    [Fact]
    public async Task VocabularyInitialization_WithChunks_ShouldProduceDifferentEmbeddings()
    {
        // Arrange - Simulate chunk-based corpus (smaller texts like actual chunks)
        var chunkCorpus = new List<string>
        {
            "### **Thomson MO5 video system**",
            "### **Address calculation**",
            "### **Graphics Mode Applications**",
            "# 🎨 Thomson MO5 C - Graphics Mode",
            "# 🎨 Thomson MO5 Assembly - Graphics Mode"
        };

        // Arrange - Simulate document-based corpus (larger texts)
        var documentCorpus = new List<string>
        {
            "# Complete Guide\n\n### **Thomson MO5 video system**\n\nDetailed content...\n\n### **Address calculation**\n\nMore content..."
        };

        var chunkService = new SimpleTfIdfEmbeddingService(_mockLogger.Object);
        var documentService = new SimpleTfIdfEmbeddingService(_mockLogger.Object);

        // Act
        await chunkService.InitializeWithCorpusAsync(chunkCorpus);
        await documentService.InitializeWithCorpusAsync(documentCorpus);

        var chunkEmbedding = await chunkService.GenerateEmbeddingAsync("graphics mode");
        var documentEmbedding = await documentService.GenerateEmbeddingAsync("graphics mode");

        // Assert
        // Embeddings from chunk-based and document-based vocabularies should differ
        var similarity = CosineSimilarity(chunkEmbedding.ToArray(), documentEmbedding.ToArray());
        Assert.True(similarity < 1.0,
            $"Embeddings from different vocabulary sources should differ (similarity: {similarity:F3})");
    }

    [Fact]
    public async Task VocabularyPersistence_AcrossMultipleQueries_ShouldRemainConsistent()
    {
        // Arrange
        var corpus = new List<string>
        {
            "graphics mode programming",
            "address calculation method",
            "Thomson MO5 video system"
        };

        await _service.InitializeWithCorpusAsync(corpus);

        // Act - Generate embeddings multiple times for the same query
        var embedding1 = await _service.GenerateEmbeddingAsync("graphics mode");
        var embedding2 = await _service.GenerateEmbeddingAsync("graphics mode");
        var embedding3 = await _service.GenerateEmbeddingAsync("graphics mode");

        // Assert - All embeddings should be identical (vocabulary is preserved)
        Assert.Equal(embedding1.ToArray(), embedding2.ToArray());
        Assert.Equal(embedding2.ToArray(), embedding3.ToArray());
    }

    [Fact]
    public async Task SearchRelevance_WithChunkBasedVocabulary_ShouldReturnHighScores()
    {
        // Arrange - Initialize with chunk-like content
        var chunks = new List<string>
        {
            "# 🎨 Thomson MO5 C - Graphics Mode",
            "### **Graphics Mode Applications**\n- [`assembly-graphics-mode.md`](assembly-graphics-mode.md)",
            "# 🎨 Thomson MO5 Assembly - Graphics Mode",
            "### **For a new graphics mode project:**\n1. Read specifications",
            "## 🖼️ Graphics errors"
        };

        await _service.InitializeWithCorpusAsync(chunks);

        // Act - Search for "graphics mode"
        var queryEmbedding = await _service.GenerateEmbeddingAsync("graphics mode");
        var chunkEmbeddings = await _service.GenerateEmbeddingsAsync(chunks.ToArray());

        // Calculate similarities
        var similarities = chunkEmbeddings
            .Select(ce => CosineSimilarity(queryEmbedding.ToArray(), ce.ToArray()))
            .ToList();

        // Assert - Should have at least one relevant similarity score
        var maxSimilarity = similarities.Max();
        Assert.True(maxSimilarity > 0.2,
            $"Should find relevant chunks (max similarity: {maxSimilarity:F3})");

        // Should have multiple relevant results
        var relevantCount = similarities.Count(s => s > 0.1);
        Assert.True(relevantCount >= 3,
            $"Should find multiple relevant chunks (found {relevantCount})");
    }

    private static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
