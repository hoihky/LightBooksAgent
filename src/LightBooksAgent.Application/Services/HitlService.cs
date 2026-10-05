using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LightBooksAgent.Application.Services;

public sealed class HitlService(AppDbContext db, IActivityLogger activityLogger) : IHitlService
{
    public async Task<ReviewRequest> CreateRequestAsync(
        Guid publishingRunId,
        ReviewGateType gateType,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken)
            ?? throw new InvalidOperationException($"Publishing run {publishingRunId} not found.");

        var request = new ReviewRequest
        {
            PublishingRunId = publishingRunId,
            GateType = gateType,
            PayloadJson = payloadJson,
            Status = ReviewStatus.Pending
        };

        run.AgentStatus = AgentRunStatus.WaitingForHuman;
        run.CurrentStep = MapGateToStep(gateType);

        db.ReviewRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);

        await activityLogger.LogAsync(
            publishingRunId,
            "Orchestrator",
            ActivityType.HitlGateReached,
            $"Waiting for human review: {gateType}",
            cancellationToken: cancellationToken);

        return request;
    }

    public async Task<ReviewRequest> ResolveAsync(
        Guid reviewRequestId,
        ReviewStatus status,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var request = await db.ReviewRequests
            .Include(r => r.PublishingRun)
            .FirstOrDefaultAsync(r => r.Id == reviewRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Review request {reviewRequestId} not found.");

        request.Status = status;
        request.HumanComment = comment;
        request.ResolvedAt = DateTimeOffset.UtcNow;

        if (request.PublishingRun is not null)
        {
            request.PublishingRun.AgentStatus = AgentRunStatus.Running;
        }

        await db.SaveChangesAsync(cancellationToken);

        await activityLogger.LogAsync(
            request.PublishingRunId,
            "Human",
            ActivityType.HitlResolved,
            $"Review {status}: {request.GateType}",
            cancellationToken: cancellationToken);

        return request;
    }

    public async Task<IReadOnlyList<ReviewRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default) =>
        await db.ReviewRequests
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    private static PublishingStep MapGateToStep(ReviewGateType gate) => gate switch
    {
        ReviewGateType.TopicConfirmation => PublishingStep.TopicProposed,
        ReviewGateType.ResearchApproval => PublishingStep.ResearchReview,
        ReviewGateType.OutlineApproval => PublishingStep.OutlineReview,
        ReviewGateType.DraftReview => PublishingStep.HumanDraftReview,
        ReviewGateType.FinalApproval => PublishingStep.FinalApproval,
        ReviewGateType.PublishConfirmation => PublishingStep.Publishing,
        ReviewGateType.UrlFetchApproval => PublishingStep.ResearchUrlApproval,
        _ => PublishingStep.ProjectCreated
    };
}
