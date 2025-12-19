using Pgvector;

namespace Mo5.RagServer.Core.Interfaces;

/// <summary>
/// Service for generating text embeddings
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generate embedding for a single text
    /// </summary>
    /// <param name="text">Text to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vector embedding</returns>
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate embeddings for multiple texts in batch
    /// </summary>
    /// <param name="texts">Texts to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vector embeddings in the same order as input</returns>
    Task<Vector[]> GenerateEmbeddingsAsync(string[] texts, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the dimension of embeddings produced by this service
    /// </summary>
    int EmbeddingDimension { get; }
    
    /// <summary>
    /// Get the maximum number of tokens that can be processed
    /// </summary>
    int MaxTokens { get; }
}
