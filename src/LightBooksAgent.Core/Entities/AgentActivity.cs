using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Entities;

public sealed class AgentActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PublishingRunId { get; set; }

    public PublishingRun? PublishingRun { get; set; }

    public string AgentName { get; set; } = string.Empty;

    public ActivityType ActivityType { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Url { get; set; }

    public int? DurationMs { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
