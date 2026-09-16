using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Models;
using Microsoft.EntityFrameworkCore;

namespace LightBooksAgent.Workflows.Orchestration;

public sealed class MafPublishingWorkflowRunner(
    AppDbContext db,
    PublishingWorkflowHost workflowHost,
    IWorkflowSessionManager sessionManager,
    IWorkflowCheckpointService checkpointService,
    IMemoryService memoryService) : IPublishingWorkflowRunner
{
    public async Task<PublishingRun> StartAsync(Guid articleProjectId, CancellationToken cancellationToken = default)
    {
        var project = await db.ArticleProjects.FindAsync([articleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleProjectId} not found.");

        if (string.IsNullOrWhiteSpace(project.ConfirmedTopic))
        {
            throw new InvalidOperationException("Confirm a topic before starting the publishing workflow.");
        }

        var run = new PublishingRun
        {
            ArticleProjectId = articleProjectId,
            CurrentStep = PublishingStep.Researching,
            AgentStatus = AgentRunStatus.Running,
            WorkflowRunId = Guid.NewGuid().ToString()
        };

        db.PublishingRuns.Add(run);
        project.Status = ArticleStatus.InProgress;
        await db.SaveChangesAsync(cancellationToken);

        run.WorkflowRunId = run.Id.ToString();
        await db.SaveChangesAsync(cancellationToken);

        await workflowHost.StartAsync(run.Id, articleProjectId, cancellationToken);
        return run;
    }

    public async Task PauseAsync(Guid publishingRunId, CancellationToken cancellationToken = default)
    {
        if (sessionManager.TryGet(publishingRunId, out var handle) && handle is not null)
        {
            await handle.StreamingRun.CancelRunAsync();
            handle.Cancellation.Cancel();
        }

        await UpdateStatusAsync(publishingRunId, AgentRunStatus.Paused, cancellationToken);
    }

    public async Task StopAsync(Guid publishingRunId, CancellationToken cancellationToken = default)
    {
        if (sessionManager.TryGet(publishingRunId, out var handle) && handle is not null)
        {
            await handle.StreamingRun.CancelRunAsync();
            handle.Cancellation.Cancel();
            sessionManager.Remove(publishingRunId);
            await handle.DisposeAsync();
        }

        await UpdateStatusAsync(publishingRunId, AgentRunStatus.Stopped, cancellationToken);
    }

    public async Task ResumeAsync(Guid publishingRunId, CancellationToken cancellationToken = default)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken)
            ?? throw new InvalidOperationException($"Run {publishingRunId} not found.");

        if (sessionManager.TryGet(publishingRunId, out var existing) && existing is not null)
        {
            await existing.StreamingRun.CancelRunAsync();
            sessionManager.Remove(publishingRunId);
        }

        var checkpoint = await checkpointService.GetLatestRunCheckpointAsync(publishingRunId, cancellationToken);
        if (checkpoint is null)
        {
            await workflowHost.StartAsync(run.Id, run.ArticleProjectId, cancellationToken);
            return;
        }

        var handle = new WorkflowSessionHandle
        {
            PublishingRunId = publishingRunId,
            SessionId = checkpoint.SessionId,
            StreamingRun = null!,
            Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken),
            LatestCheckpoint = checkpoint
        };

        sessionManager.Register(handle);
        await workflowHost.ResumeAsync(handle, cancellationToken);
        await UpdateStatusAsync(publishingRunId, AgentRunStatus.Running, cancellationToken);
    }

    public async Task<PublishingRun?> GetRunAsync(Guid publishingRunId, CancellationToken cancellationToken = default) =>
        await db.PublishingRuns
            .Include(r => r.Activities.OrderByDescending(a => a.Timestamp).Take(50))
            .Include(r => r.ResearchMaterials)
            .Include(r => r.ReviewRequests)
            .FirstOrDefaultAsync(r => r.Id == publishingRunId, cancellationToken);

    public async Task<IReadOnlyList<PublishingRun>> GetActiveRunsAsync(CancellationToken cancellationToken = default) =>
        await db.PublishingRuns
            .Where(r => r.AgentStatus == AgentRunStatus.Running ||
                        r.AgentStatus == AgentRunStatus.WaitingForHuman ||
                        r.AgentStatus == AgentRunStatus.Paused)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task AdvanceAfterReviewAsync(
        Guid publishingRunId,
        ReviewGateType gateType,
        bool approved,
        CancellationToken cancellationToken = default)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken)
            ?? throw new InvalidOperationException($"Run {publishingRunId} not found.");

        if (!approved)
        {
            run.RevisionCount = gateType == ReviewGateType.DraftReview
                ? run.RevisionCount + 1
                : run.RevisionCount;
        }

        run.AgentStatus = AgentRunStatus.Running;
        await db.SaveChangesAsync(cancellationToken);

        var latestReview = await db.ReviewRequests
            .Where(r => r.PublishingRunId == publishingRunId && r.GateType == gateType)
            .OrderByDescending(r => r.ResolvedAt ?? r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var comment = latestReview?.HumanComment;
        if (!string.IsNullOrWhiteSpace(comment))
        {
            await memoryService.DistillFromFeedbackAsync(
                run.ArticleProjectId,
                comment,
                cancellationToken);
        }

        await workflowHost.SubmitReviewResponseAsync(
            publishingRunId,
            new ReviewResponsePayload(approved, comment),
            cancellationToken);
    }

    private async Task UpdateStatusAsync(
        Guid publishingRunId,
        AgentRunStatus status,
        CancellationToken cancellationToken)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run is null)
        {
            return;
        }

        run.AgentStatus = status;
        await db.SaveChangesAsync(cancellationToken);
    }
}
