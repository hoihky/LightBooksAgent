namespace LightBooksAgent.Core.Entities;

public sealed class ResearchMaterial
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PublishingRunId { get; set; }

    public PublishingRun? PublishingRun { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int RelevanceScore { get; set; }

    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;
}
