using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Interfaces;
using Pgvector;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Local embedding service using Sentence Transformers
/// No external API calls required!
/// </summary>
public class LocalEmbeddingService : IEmbeddingService, IDisposable
{
    private readonly ILogger<LocalEmbeddingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _modelName = string.Empty;
    private readonly string _pythonPath = string.Empty;
    private readonly object _lock = new();
    private bool _isInitialized = false;
    private bool _disposed = false;

    public int EmbeddingDimension => 384; // paraphrase-multilingual-MiniLM-L12-v2 produit 384 dimensions
    public int MaxTokens => 512; // Limite du modèle

    public LocalEmbeddingService(ILogger<LocalEmbeddingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Configuration du modèle (multilingue et optimisé)
        _modelName = _configuration.GetValue<string>("LocalEmbedding:ModelName", "paraphrase-multilingual-MiniLM-L12-v2") ?? "paraphrase-multilingual-MiniLM-L12-v2";
        _pythonPath = _configuration.GetValue<string>("LocalEmbedding:PythonPath", "python") ?? "python";
        
        _logger.LogInformation("Initializing Local Embedding Service with model: {ModelName}", _modelName);
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        await EnsureInitializedAsync(cancellationToken);

        try
        {
            var embedding = await GenerateEmbeddingInternalAsync(text, cancellationToken);
            return new Vector(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    public async Task<Vector[]> GenerateEmbeddingsAsync(string[] texts, CancellationToken cancellationToken = default)
    {
        if (texts == null || texts.Length == 0)
            return Array.Empty<Vector>();

        await EnsureInitializedAsync(cancellationToken);

        try
        {
            _logger.LogDebug("Generating embeddings for {Count} texts", texts.Length);

            var embeddings = await GenerateEmbeddingsBatchAsync(texts.ToList(), cancellationToken);
            return embeddings.Select(e => new Vector(e)).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings for {Count} texts", texts.Length);
            throw;
        }
    }

    private Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized) return Task.CompletedTask;

        lock (_lock)
        {
            if (_isInitialized) return Task.CompletedTask;

            try
            {
                _logger.LogInformation("Initializing local embedding model...");

                // Vérifier que Python est disponible
                if (!IsPythonAvailable())
                {
                    throw new InvalidOperationException("Python is not available. Please install Python 3.8+ and ensure it's in PATH.");
                }

                // Vérifier/installer les dépendances Python
                EnsurePythonDependencies();

                _isInitialized = true;
                _logger.LogInformation("Local embedding service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize local embedding service");
                throw;
            }
        }

        return Task.CompletedTask;
    }

    private bool IsPythonAvailable()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(5000);
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private void EnsurePythonDependencies()
    {
        var requiredPackages = new[] { "sentence-transformers", "torch", "numpy" };
        
        foreach (var package in requiredPackages)
        {
            if (!IsPythonPackageInstalled(package))
            {
                _logger.LogInformation("Installing Python package: {Package}", package);
                InstallPythonPackage(package);
            }
        }
    }

    private bool IsPythonPackageInstalled(string packageName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"-c \"import {packageName.Replace("-", "_")}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(10000);
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private void InstallPythonPackage(string packageName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = $"-m pip install {packageName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit(120000); // 2 minutes timeout

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to install {packageName}: {error}");
        }

        _logger.LogInformation("Successfully installed {Package}", packageName);
    }

    private async Task<float[]> GenerateEmbeddingInternalAsync(string text, CancellationToken cancellationToken)
    {
        var pythonScript = CreatePythonScript(new[] { text });
        var result = await ExecutePythonScriptAsync(pythonScript, cancellationToken);
        
        var embeddings = JsonSerializer.Deserialize<float[][]>(result);
        return embeddings?[0] ?? throw new InvalidOperationException("Failed to parse embedding result");
    }

    private async Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts, CancellationToken cancellationToken)
    {
        var pythonScript = CreatePythonScript(texts);
        var result = await ExecutePythonScriptAsync(pythonScript, cancellationToken);
        
        var embeddings = JsonSerializer.Deserialize<float[][]>(result);
        return embeddings?.ToList() ?? throw new InvalidOperationException("Failed to parse embeddings result");
    }

    private string CreatePythonScript(IEnumerable<string> texts)
    {
        var textsJson = JsonSerializer.Serialize(texts);
        
        return $@"
import json
import sys
from sentence_transformers import SentenceTransformer

try:
    # Load model (cached after first use)
    model = SentenceTransformer('{_modelName}')
    
    # Input texts
    texts = {textsJson}
    
    # Generate embeddings
    embeddings = model.encode(texts, convert_to_numpy=True)
    
    # Convert to list and output as JSON
    result = embeddings.tolist()
    print(json.dumps(result))
    
except Exception as e:
    print(f'Error: {{e}}', file=sys.stderr)
    sys.exit(1)
";
    }

    private async Task<string> ExecutePythonScriptAsync(string script, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                Arguments = "-c",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        var tcs = new TaskCompletionSource<string>();
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null) output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) error.AppendLine(e.Data);
        };

        process.Exited += (sender, e) =>
        {
            if (process.ExitCode == 0)
            {
                tcs.SetResult(output.ToString().Trim());
            }
            else
            {
                tcs.SetException(new InvalidOperationException($"Python script failed: {error}"));
            }
        };

        process.EnableRaisingEvents = true;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Write script to stdin
        await process.StandardInput.WriteAsync(script);
        process.StandardInput.Close();

        // Wait for completion with cancellation support
        using (cancellationToken.Register(() => 
        {
            try { process.Kill(); } catch { }
            tcs.TrySetCanceled();
        }))
        {
            return await tcs.Task;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
