namespace Mo5.RagServer.Core.Entities;

public class OfficialDocument
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime LastModified { get; set; }
}