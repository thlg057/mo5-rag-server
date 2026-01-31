using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Models;

namespace Mo5.RagServer.Core.Interfaces;

/// <summary>
/// Service for managing documents and performing semantic search
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Index all documents from the knowledge base directory
    /// </summary>
    /// <param name="knowledgeBasePath">Path to the knowledge base directory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of documents processed</returns>
    Task<int> IndexDocumentsAsync(string knowledgeBasePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Index a single document
    /// </summary>
    /// <param name="filePath">Path to the document file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The indexed document</returns>
    Task<Document> IndexDocumentAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Perform semantic search across all documents
    /// </summary>
    /// <param name="request">Search request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all documents with optional filtering
    /// </summary>
    /// <param name="tags">Filter by tags (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of documents</returns>
    Task<List<Document>> GetDocumentsAsync(List<string>? tags = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document or null if not found</returns>
    Task<Document?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a document and all its chunks
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all available tags
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tags</returns>
    Task<List<Tag>> GetTagsAsync(CancellationToken cancellationToken = default);
}
