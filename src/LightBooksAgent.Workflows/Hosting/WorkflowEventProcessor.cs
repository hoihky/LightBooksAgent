using System.Text.Json;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LightBooksAgent.Workflows.Hosting;

public sealed class WorkflowEventProcessor(
    IServiceScopeFactory scopeFactory,
    IWorkflowSessionManager sessionManager,
    IWorkflowCheckpointService checkpointService,
    ILogger<WorkflowEventProcessor> logger) : IWorkflowEventProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public async Task ProcessAsync(
        WorkflowSessionHandle session,
        WorkflowEvent workflowEvent,
        CancellationToken cancellationToken = default)
    {
        switch (workflowEvent)
        {
            case RequestInfoEvent requestInfo:
                await HandleRequestInfoAsync(session, requestInfo, cancellationToken);
                break;

            case SuperStepCompletedEvent superStep when superStep.CompletionInfo?.Checkpoint is not null:
                session.LatestCheckpoint = superStep.CompletionInfo.Checkpoint;
                await checkpointService.PersistRunCheckpointAsync(
                    session.PublishingRunId,
                    superStep.CompletionInfo.Checkpoint,
                    cancellationToken);
                break;

            case WorkflowOutputEvent output when output.Data is PublishingWorkflowState completedState:
                await HandleCompletionAsync(session, completedState, cancellationToken);
                break;

            case ExecutorInvokedEvent invoked:
                await UpdateRunActivityAsync(
                    session.PublishingRunId,
                    invoked.ExecutorId,
                    AgentRunStatus.Running,
                    $"Executing {invoked.ExecutorId}",
                    cancellationToken);
                break;

            case ExecutorCompletedEvent completed:
                logger.LogDebug(
                    "Executor {ExecutorId} completed for run {RunId}",
                    completed.ExecutorId,
                    session.PublishingRunId);
                break;

            case ExecutorFailedEvent failed:
                await HandleFailureAsync(session, failed.ExecutorId, failed.Data?.ToString(), cancellationToken);
                break;
        }
    }

    private async Task HandleRequestInfoAsync(
        WorkflowSessionHandle session,
        RequestInfoEvent requestInfo,
        CancellationToken cancellationToken)
    {
        if (!requestInfo.Request.TryGetDataAs<ReviewRequestPayload>(out var payload))
        {
            logger.LogWarning("Received HITL request with unexpected payload for run {RunId}", session.PublishingRunId);
            return;
        }

        sessionManager.SetPendingRequest(session.PublishingRunId, requestInfo.Request, payload.GateType);

        await using var scope = scopeFactory.CreateAsyncScope();
        var hitlService = scope.ServiceProvider.GetRequiredService<IHitlService>();
        var activityLogger = scope.ServiceProvider.GetRequiredService<IActivityLogger>();

        await hitlService.CreateRequestAsync(
            session.PublishingRunId,
            payload.GateType,
            JsonSerializer.Serialize(payload, JsonOptions),
            cancellationToken);

        await UpdateRunActivityAsync(
            session.PublishingRunId,
            "Orchestrator",
            AgentRunStatus.WaitingForHuman,
            $"Waiting for human review: {payload.GateType}",
            cancellationToken);

        await activityLogger.LogAsync(
            session.PublishingRunId,
            "Orchestrator",
            ActivityType.HitlGateReached,
            $"HITL gate reached: {payload.GateType}",
            cancellationToken: cancellationToken);
    }

    private async Task HandleCompletionAsync(
        WorkflowSessionHandle session,
        PublishingWorkflowState state,
        CancellationToken cancellationToken)
    {
        await UpdateRunActivityAsync(
            session.PublishingRunId,
            "Orchestrator",
            AgentRunStatus.Completed,
            "Publishing workflow completed",
            cancellationToken,
            PublishingStep.Published);

        sessionManager.Remove(session.PublishingRunId);
    }

    private async Task HandleFailureAsync(
        WorkflowSessionHandle session,
        string executorId,
        string? error,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = await db.PublishingRuns.FindAsync([session.PublishingRunId], cancellationToken);
        if (run is not null)
        {
            run.AgentStatus = AgentRunStatus.Error;
            run.CurrentStep = PublishingStep.Failed;
            run.ErrorMessage = error ?? $"Executor {executorId} failed.";
            run.CurrentAgentName = executorId;
            await db.SaveChangesAsync(cancellationToken);
        }

        sessionManager.Remove(session.PublishingRunId);
    }

    private async Task UpdateRunActivityAsync(
        Guid publishingRunId,
        string agentName,
        AgentRunStatus status,
        string activity,
        CancellationToken cancellationToken,
        PublishingStep? step = null)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run is null)
        {
            return;
        }

        run.AgentStatus = status;
        run.CurrentAgentName = agentName;
        run.CurrentActivity = activity;
        if (step.HasValue)
        {
            run.CurrentStep = step.Value;
        }

        if (status == AgentRunStatus.Completed)
        {
            run.CompletedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
