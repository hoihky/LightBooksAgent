using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IWorkflowCheckpointService
{
    CheckpointManager Manager { get; }

    Task PersistRunCheckpointAsync(
        Guid publishingRunId,
        CheckpointInfo checkpoint,
        CancellationToken cancellationToken = default);

    Task<CheckpointInfo?> GetLatestRunCheckpointAsync(
        Guid publishingRunId,
        CancellationToken cancellationToken = default);
}
