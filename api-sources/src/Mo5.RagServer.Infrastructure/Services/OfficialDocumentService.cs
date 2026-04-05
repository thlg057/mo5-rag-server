using System.Text.Json;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Interfaces;

namespace Mo5.RagServer.Infrastructure.Services;

public class OfficialDocumentService : IOfficialDocumentService
{
    private readonly string _officialPath;
    private readonly string _jsonPath;
    private readonly ILogger<OfficialDocumentService> _logger;

    // On passe directement le chemin vers le dossier "official"
    public OfficialDocumentService(string officialPath, ILogger<OfficialDocumentService> logger)
    {
        _officialPath = officialPath;
        _jsonPath = Path.Combine(officialPath, "docs.json");
        _logger = logger;
    }

    private async Task<List<OfficialDocument>> LoadIndexAsync()
    {
        if (!File.Exists(_jsonPath))
        {
            _logger.LogWarning("Index JSON absent : {Path}", _jsonPath);
            return new List<OfficialDocument>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_jsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<Dictionary<string, List<OfficialDocument>>>(json, options);
            return data?["documents"] ?? new List<OfficialDocument>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lecture docs.json");
            return new List<OfficialDocument>();
        }
    }

    public async Task<List<OfficialDocument>> GetOfficialDocumentsAsync(CancellationToken ct = default) 
        => await LoadIndexAsync();

    public async Task<OfficialDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var docs = await LoadIndexAsync();
        return docs.FirstOrDefault(d => d.Id == id);
    }

    public async Task<(byte[] content, string contentType, string fileName)> GetFileContentAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await GetByIdAsync(id, ct);
        if (doc == null) throw new FileNotFoundException();

        // On part de la racine du dossier officiel pour trouver le fichier
        var fullPath = Path.Combine(_officialPath, Path.GetFileName(doc.FilePath));

        var content = await File.ReadAllBytesAsync(fullPath, ct);
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var type = ext == ".pdf" ? "application/pdf" : "text/markdown";

        return (content, type, doc.FileName);
    }
}