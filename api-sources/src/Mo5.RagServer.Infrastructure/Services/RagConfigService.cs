using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Services;

namespace Mo5.RagServer.Infrastructure.Services;

public class RagConfigService : IRagConfigService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RagConfigService> _logger;

    private readonly Lazy<TagMappingConfig> _tagConfig;
    private readonly Lazy<RerankingConfig> _rerankConfig;

    public RagConfigService(
        IConfiguration configuration,
        ILogger<RagConfigService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _tagConfig = new Lazy<TagMappingConfig>(
            LoadTagMapping,
            LazyThreadSafetyMode.ExecutionAndPublication);

        _rerankConfig = new Lazy<RerankingConfig>(
            LoadReranking,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public TagMappingConfig GetTagMapping() => _tagConfig.Value;

    public RerankingConfig GetReranking() => _rerankConfig.Value;

    // --------------------------------------------------
    // LOADERS
    // --------------------------------------------------

    private TagMappingConfig LoadTagMapping()
    {
        try
        {
            var path = GetConfigPath("tag-mapping.json");

            if (!File.Exists(path))
            {
                _logger.LogWarning("Tag mapping config not found at {Path}, using default.", path);
                return GetDefaultTagMapping();
            }

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<TagMappingConfig>(json);

            return config ?? GetDefaultTagMapping();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tag mapping config.");
            return GetDefaultTagMapping();
        }
    }

    private RerankingConfig LoadReranking()
    {
        try
        {
            var path = GetConfigPath("reranking.json");

            if (!File.Exists(path))
            {
                _logger.LogWarning("Reranking config not found at {Path}, using default.", path);
                return GetDefaultReranking();
            }

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<RerankingConfig>(json);

            return config ?? GetDefaultReranking();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load reranking config.");
            return GetDefaultReranking();
        }
    }

    // --------------------------------------------------
    // HELPERS
    // --------------------------------------------------

    private string GetConfigPath(string fileName)
    {
        var basePath = _configuration.GetValue<string>("ContentRootPath") ?? "content";
        return Path.Combine(basePath, "config", "rag", fileName);
    }

    // --------------------------------------------------
    // DEFAULTS (fallback)
    // --------------------------------------------------

    private TagMappingConfig GetDefaultTagMapping()
    {
        return new TagMappingConfig
        {
            Mapping = new Dictionary<string, List<string>>
            {
                ["collisions"] = ["collision", "hitbox", "overlap"],
                ["performance"] = ["slow", "rame", "lent", "performance", "fps"],
                ["rendering"] = ["sprite", "draw", "render"]
            }
        };
    }

    private RerankingConfig GetDefaultReranking()
    {
        return new RerankingConfig
        {
            BoostRules = new List<BoostRule>
            {
                new() { Keywords = ["problem", "problème"], Boost = 0.03f },
                new() { Keywords = ["solution", "fix"], Boost = 0.05f },
                new() { Keywords = ["optimize", "optimisation"], Boost = 0.04f }
            }
        };
    }
}