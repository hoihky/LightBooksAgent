using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Workflows.Models;

public sealed class PublishingWorkflowState
{
    public Guid PublishingRunId { get; init; }

    public Guid ArticleProjectId { get; init; }

    public PublishingStep CurrentStep { get; set; } = PublishingStep.Researching;

    public string? ResearchBrief { get; set; }

    public string? OutlineMarkdown { get; set; }

    public string? LatestDraft { get; set; }

    public bool LastReviewApproved { get; set; }

    public ReviewGateType? PendingGate { get; set; }

    public string? HumanComment { get; set; }

    public int RevisionCount { get; set; }

    public bool ExportHtml { get; set; }

    public bool UseWeChatTheme { get; set; }

    public bool IsCompleted { get; set; }

    public List<string> ProposedResearchUrls { get; set; } = [];

    public List<string> ApprovedResearchUrls { get; set; } = [];

    public static PublishingWorkflowState CreateStart(Guid publishingRunId, Guid articleProjectId) =>
        new()
        {
            PublishingRunId = publishingRunId,
            ArticleProjectId = articleProjectId,
            CurrentStep = PublishingStep.Researching
        };
}
