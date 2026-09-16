using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Interfaces;

public interface IPublishingWorkflowRunner
{
    Task<PublishingRun> StartAsync(Guid articleProjectId, CancellationToken cancellationToken = default);

    Task PauseAsync(Guid publishingRunId, CancellationToken cancellationToken = default);

    Task StopAsync(Guid publishingRunId, CancellationToken cancellationToken = default);

    Task ResumeAsync(Guid publishingRunId, CancellationToken cancellationToken = default);

    Task<PublishingRun?> GetRunAsync(Guid publishingRunId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublishingRun>> GetActiveRunsAsync(CancellationToken cancellationToken = default);

    Task AdvanceAfterReviewAsync(
        Guid publishingRunId,
        ReviewGateType gateType,
        bool approved,
        CancellationToken cancellationToken = default);
}
