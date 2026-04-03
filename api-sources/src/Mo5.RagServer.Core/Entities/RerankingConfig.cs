namespace Mo5.RagServer.Core.Entities;

public class RerankingConfig
{
    public List<BoostRule> BoostRules { get; set; } = new();
}

public class BoostRule
{
    public List<string> Keywords { get; set; } = new();
    public float Boost { get; set; }
}