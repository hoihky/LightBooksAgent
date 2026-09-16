using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Entities;

public sealed class ArticleProject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public ArticleCategory Category { get; set; }

    public string Audience { get; set; } = string.Empty;

    public string SeedKeywords { get; set; } = string.Empty;

    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>JSON array of human-proposed topic ideas.</summary>
    public string ProposedTopicsJson { get; set; } = "[]";

    public string? ConfirmedTopic { get; set; }

    public string? ResearchBrief { get; set; }

    public string? OutlineMarkdown { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PublishingRun> Runs { get; set; } = [];

    public ICollection<ArticleVersion> Versions { get; set; } = [];

    public ICollection<AgentMemoryEntry> Memories { get; set; } = [];
}
