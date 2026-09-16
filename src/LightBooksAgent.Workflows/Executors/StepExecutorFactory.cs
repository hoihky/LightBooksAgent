using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Executors;

public sealed class StepExecutorFactory(IPublishingStepDispatcher dispatcher)
{
    public FunctionExecutor<PublishingWorkflowState, PublishingWorkflowState> Create(
        string executorId,
        PublishingStep step) =>
        new(
            executorId,
            async (state, _, cancellationToken) =>
            {
                var dispatchStep = state.CurrentStep == PublishingStep.Revision
                    ? PublishingStep.Revision
                    : step;

                if (dispatchStep != PublishingStep.Revision)
                {
                    state.CurrentStep = step;
                }

                return await dispatcher.DispatchAsync(dispatchStep, state, cancellationToken);
            },
            declareCrossRunShareable: true);

    public FunctionExecutor<WorkflowStartInput, PublishingWorkflowState> CreateInitializer() =>
        new(
            "initialize",
            (input, _, _) => ValueTask.FromResult(
                PublishingWorkflowState.CreateStart(input.PublishingRunId, input.ArticleProjectId)),
            declareCrossRunShareable: true);

    public FunctionExecutor<PublishingWorkflowState, PublishingWorkflowState> CreateCompletionExecutor() =>
        new(
            "complete",
            (state, _, _) =>
            {
                state.IsCompleted = true;
                state.CurrentStep = PublishingStep.Published;
                return ValueTask.FromResult(state);
            },
            declareCrossRunShareable: true);
}
