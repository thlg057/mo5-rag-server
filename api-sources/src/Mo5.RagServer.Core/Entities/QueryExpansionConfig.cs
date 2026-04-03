namespace Mo5.RagServer.Core.Entities;

public class QueryExpansionConfig
{
    public MultiQueryConfig MultiQuery { get; set; } = new();
}

public class MultiQueryConfig
{
    public int MaxQueries { get; set; }
    public List<QueryRule> Rules { get; set; } = new();
}

public class QueryRule
{
    public string Intent { get; set; } = "";
    public List<string> Keywords { get; set; } = new();
    public List<string> Expansions { get; set; } = new();
}