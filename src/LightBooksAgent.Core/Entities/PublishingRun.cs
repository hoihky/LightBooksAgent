using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Entities;

public sealed class PublishingRun
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ArticleProjectId { get; set; }

    public ArticleProject? ArticleProject { get; set; }

    public string? WorkflowRunId { get; set; }

    public PublishingStep CurrentStep { get; set; } = PublishingStep.ProjectCreated;

    public AgentRunStatus AgentStatus { get; set; } = AgentRunStatus.Idle;

    public string? CurrentAgentName { get; set; }

    public string? CurrentActivity { get; set; }

    public string? CheckpointId { get; set; }

    public int RevisionCount { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public ICollection<AgentActivity> Activities { get; set; } = [];

    public ICollection<ResearchMaterial> ResearchMaterials { get; set; } = [];

    public ICollection<ReviewRequest> ReviewRequests { get; set; } = [];
}
