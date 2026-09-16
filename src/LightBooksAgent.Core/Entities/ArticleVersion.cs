namespace LightBooksAgent.Core.Entities;

public sealed class ArticleVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ArticleProjectId { get; set; }

    public ArticleProject? ArticleProject { get; set; }

    public int VersionNumber { get; set; }

    public string ContentMarkdown { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public string? ChangeSummary { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
