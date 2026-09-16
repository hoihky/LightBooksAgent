using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace LightBooksAgent.Workflows.Hosting;

public sealed class PublishingWorkflowHost(
    IPublishingWorkflowFactory workflowFactory,
    IWorkflowCheckpointService checkpointService,
    IWorkflowSessionManager sessionManager,
    IWorkflowEventProcessor eventProcessor,
    ILogger<PublishingWorkflowHost> logger)
{
    public async Task<WorkflowSessionHandle> StartAsync(
        Guid publishingRunId,
        Guid articleProjectId,
        CancellationToken cancellationToken = default)
    {
        var workflow = workflowFactory.CreateWorkflow();
        var sessionId = publishingRunId.ToString();
        var input = new WorkflowStartInput(publishingRunId, articleProjectId);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var streamingRun = await InProcessExecution
            .OffThread
            .WithCheckpointing(checkpointService.Manager)
            .RunStreamingAsync(workflow, input, sessionId, cts.Token);

        var handle = new WorkflowSessionHandle
        {
            PublishingRunId = publishingRunId,
            SessionId = sessionId,
            StreamingRun = streamingRun,
            Cancellation = cts
        };

        sessionManager.Register(handle);
        _ = PumpEventsAsync(handle);
        return handle;
    }

    public async Task ResumeAsync(WorkflowSessionHandle handle, CancellationToken cancellationToken = default)
    {
        if (handle.LatestCheckpoint is null)
        {
            throw new InvalidOperationException("Cannot resume workflow without a checkpoint.");
        }

        var workflow = workflowFactory.CreateWorkflow();
        var resumedRun = await InProcessExecution
            .OffThread
            .WithCheckpointing(checkpointService.Manager)
            .ResumeStreamingAsync(workflow, handle.LatestCheckpoint, cancellationToken);

        await handle.StreamingRun.DisposeAsync();
        handle.StreamingRun = resumedRun;
        _ = PumpEventsAsync(handle);
    }

    public async Task SubmitReviewResponseAsync(
        Guid publishingRunId,
        ReviewResponsePayload response,
        CancellationToken cancellationToken = default)
    {
        if (!sessionManager.TryGet(publishingRunId, out var handle) || handle?.PendingRequest is null)
        {
            throw new InvalidOperationException("No pending HITL request for this publishing run.");
        }

        var externalResponse = handle.PendingRequest.CreateResponse(response);
        await handle.StreamingRun.SendResponseAsync(externalResponse);
        handle.PendingRequest = null;
        handle.PendingGate = null;

        _ = PumpEventsAsync(handle);
    }

    private async Task PumpEventsAsync(WorkflowSessionHandle handle)
    {
        try
        {
            await foreach (var workflowEvent in handle.StreamingRun.WatchStreamAsync(
                               blockOnPendingRequest: false,
                               handle.Cancellation.Token))
            {
                await eventProcessor.ProcessAsync(handle, workflowEvent, handle.Cancellation.Token);

                if (workflowEvent is RequestInfoEvent)
                {
                    break;
                }

                var status = await handle.StreamingRun.GetStatusAsync(handle.Cancellation.Token);
                if (status is RunStatus.PendingRequests or RunStatus.Idle or RunStatus.Ended)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Workflow event pump cancelled for run {RunId}", handle.PublishingRunId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Workflow event pump failed for run {RunId}", handle.PublishingRunId);
        }
    }
}
