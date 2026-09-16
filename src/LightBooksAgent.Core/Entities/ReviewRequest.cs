using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Entities;

public sealed class ReviewRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PublishingRunId { get; set; }

    public PublishingRun? PublishingRun { get; set; }

    public ReviewGateType GateType { get; set; }

    public string PayloadJson { get; set; } = "{}";

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public string? HumanComment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }
}
