using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Pgvector;
using System.Text.RegularExpressions;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Simple TF-IDF based embedding service
/// No external dependencies - pure C# implementation
/// </summary>
public class SimpleTfIdfEmbeddingService : IEmbeddingService
{
    private readonly ILogger<SimpleTfIdfEmbeddingService> _logger;
    private readonly Dictionary<string, int> _vocabulary = new();
    private readonly Dictionary<string, double> _idfScores = new();
    private readonly object _lock = new();
    private bool _isInitialized = false;
    private readonly int _vectorSize = 384; // Taille fixe pour compatibilité

    // Mots vides français et anglais
    private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Français
        "le", "de", "et", "à", "un", "il", "être", "et", "en", "avoir", "que", "pour",
        "dans", "ce", "son", "une", "sur", "avec", "ne", "se", "pas", "tout", "plus",
        "par", "grand", "en", "me", "même", "elle", "vous", "ou", "du", "au", "nous",
        "comme", "mais", "pouvoir", "dire", "votre", "si", "ces", "son", "mes", "nos",
        
        // Anglais
        "the", "be", "to", "of", "and", "a", "in", "that", "have", "i", "it", "for",
        "not", "on", "with", "he", "as", "you", "do", "at", "this", "but", "his", "by",
        "from", "they", "we", "say", "her", "she", "or", "an", "will", "my", "one", "all",
        
        // Techniques
        "code", "function", "method", "class", "var", "int", "string", "bool", "void"
    };

    public int EmbeddingDimension => _vectorSize;
    public int MaxTokens => 8192; // Pas de limite réelle pour TF-IDF

    public SimpleTfIdfEmbeddingService(ILogger<SimpleTfIdfEmbeddingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Initialized Simple TF-IDF Embedding Service (vector size: {VectorSize})", _vectorSize);
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        await Task.CompletedTask; // Pour respecter l'interface async

        var embedding = GenerateEmbedding(text);
        return new Vector(embedding);
    }

    public async Task<Vector[]> GenerateEmbeddingsAsync(string[] texts, CancellationToken cancellationToken = default)
    {
        if (texts == null || texts.Length == 0)
            return Array.Empty<Vector>();

        await Task.CompletedTask; // Pour respecter l'interface async

        // Construire le vocabulaire si nécessaire
        EnsureVocabularyBuilt(texts.ToList());

        var embeddings = texts.Select(GenerateEmbedding).Select(e => new Vector(e)).ToArray();

        _logger.LogDebug("Generated {Count} TF-IDF embeddings", embeddings.Length);
        return embeddings;
    }

    private void EnsureVocabularyBuilt(List<string> texts)
    {
        lock (_lock)
        {
            if (_isInitialized) return;

            _logger.LogInformation("Building TF-IDF vocabulary from {Count} texts...", texts.Count);

            // Construire le vocabulaire
            var allTerms = new HashSet<string>();
            var documentTerms = new List<List<string>>();

            foreach (var text in texts)
            {
                var terms = TokenizeAndFilter(text);
                documentTerms.Add(terms);
                foreach (var term in terms)
                {
                    allTerms.Add(term);
                }
            }

            // Assigner des indices aux termes
            var termIndex = 0;
            foreach (var term in allTerms.OrderBy(t => t))
            {
                if (termIndex >= _vectorSize) break; // Limiter la taille du vocabulaire
                _vocabulary[term] = termIndex++;
            }

            // Calculer les scores IDF
            var totalDocuments = documentTerms.Count;
            foreach (var term in _vocabulary.Keys)
            {
                var documentsContainingTerm = documentTerms.Count(doc => doc.Contains(term));
                _idfScores[term] = Math.Log((double)totalDocuments / (1 + documentsContainingTerm));
            }

            _isInitialized = true;
            _logger.LogInformation("TF-IDF vocabulary built: {VocabSize} terms", _vocabulary.Count);
        }
    }

    private float[] GenerateEmbedding(string text)
    {
        var vector = new float[_vectorSize];
        var terms = TokenizeAndFilter(text);
        
        if (!terms.Any()) return vector;

        // Calculer TF (Term Frequency)
        var termFrequency = new Dictionary<string, int>();
        foreach (var term in terms)
        {
            termFrequency[term] = termFrequency.GetValueOrDefault(term, 0) + 1;
        }

        // Calculer TF-IDF pour chaque terme du vocabulaire
        var totalTerms = terms.Count;
        foreach (var kvp in _vocabulary)
        {
            var term = kvp.Key;
            var index = kvp.Value;
            
            if (index >= _vectorSize) continue;

            if (termFrequency.ContainsKey(term))
            {
                var tf = (double)termFrequency[term] / totalTerms;
                var idf = _idfScores.GetValueOrDefault(term, 0);
                vector[index] = (float)(tf * idf);
            }
        }

        // Normaliser le vecteur
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] / magnitude);
            }
        }

        return vector;
    }

    private List<string> TokenizeAndFilter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // Nettoyer et tokeniser
        var cleaned = Regex.Replace(text.ToLowerInvariant(), @"[^\w\s]", " ");
        var tokens = Regex.Split(cleaned, @"\s+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Where(token => token.Length >= 2) // Mots d'au moins 2 caractères
            .Where(token => !_stopWords.Contains(token))
            .Where(token => !IsNumeric(token))
            .ToList();

        return tokens;
    }

    private static bool IsNumeric(string token)
    {
        return double.TryParse(token, out _);
    }

    /// <summary>
    /// Initialise le service avec un corpus de documents pour construire le vocabulaire
    /// </summary>
    public async Task InitializeWithCorpusAsync(IEnumerable<string> corpus)
    {
        var corpusList = corpus.ToList();
        _logger.LogInformation("Initializing TF-IDF with corpus of {Count} documents", corpusList.Count);
        
        EnsureVocabularyBuilt(corpusList);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtient des statistiques sur le vocabulaire
    /// </summary>
    public Dictionary<string, object> GetVocabularyStats()
    {
        lock (_lock)
        {
            return new Dictionary<string, object>
            {
                ["VocabularySize"] = _vocabulary.Count,
                ["VectorSize"] = _vectorSize,
                ["IsInitialized"] = _isInitialized,
                ["TopTerms"] = _idfScores
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(20)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }
}
