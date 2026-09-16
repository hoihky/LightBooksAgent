using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Workflows.Abstractions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace LightBooksAgent.Workflows.Hosting;

public sealed class WorkflowCheckpointService(
    AppDbContext db,
    WorkflowCheckpointManagerProvider checkpointManagerProvider,
    ILogger<WorkflowCheckpointService> logger) : IWorkflowCheckpointService
{
    public CheckpointManager Manager => checkpointManagerProvider.Manager;

    public async Task PersistRunCheckpointAsync(
        Guid publishingRunId,
        CheckpointInfo checkpoint,
        CancellationToken cancellationToken = default)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run is null)
        {
            return;
        }

        run.CheckpointId = checkpoint.CheckpointId;
        run.WorkflowRunId = checkpoint.SessionId;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogDebug(
            "Persisted checkpoint {CheckpointId} for run {RunId}",
            checkpoint.CheckpointId,
            publishingRunId);
    }

    public async Task<CheckpointInfo?> GetLatestRunCheckpointAsync(
        Guid publishingRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run?.CheckpointId is null || run.WorkflowRunId is null)
        {
            return null;
        }

        return new CheckpointInfo(run.WorkflowRunId, run.CheckpointId);
    }
}
