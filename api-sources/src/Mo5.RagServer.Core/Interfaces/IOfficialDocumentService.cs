using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Core.Interfaces;

public interface IOfficialDocumentService
{
    Task<List<OfficialDocument>> GetOfficialDocumentsAsync(CancellationToken ct = default);
    Task<OfficialDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(byte[] content, string contentType, string fileName)> GetFileContentAsync(Guid id, CancellationToken ct = default);
}