using LightBooksAgent.Core.Enums;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Models;

public sealed class WorkflowSessionHandle : IAsyncDisposable
{
    public required Guid PublishingRunId { get; init; }

    public required string SessionId { get; init; }

    public required StreamingRun StreamingRun { get; set; }

    public required CancellationTokenSource Cancellation { get; init; }

    public ExternalRequest? PendingRequest { get; set; }

    public ReviewGateType? PendingGate { get; set; }

    public CheckpointInfo? LatestCheckpoint { get; set; }

    public async ValueTask DisposeAsync()
    {
        Cancellation.Cancel();
        await StreamingRun.DisposeAsync();
    }
}
