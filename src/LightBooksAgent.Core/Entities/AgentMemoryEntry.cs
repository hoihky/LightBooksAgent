using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Entities;

public sealed class AgentMemoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ArticleProjectId { get; set; }

    public ArticleProject? ArticleProject { get; set; }

    public string AgentName { get; set; } = string.Empty;

    public MemoryLayer Layer { get; set; }

    public ArticleCategory? Category { get; set; }

    public string Content { get; set; } = string.Empty;

    public byte[]? Embedding { get; set; }

    public float Importance { get; set; } = 0.5f;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
