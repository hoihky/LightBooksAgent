using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Steps;

public sealed class RevisionStepHandler(WritingStepHandler writingStepHandler) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.Revision;

    public Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        state.CurrentStep = PublishingStep.Revision;
        return writingStepHandler.ExecuteAsync(state, cancellationToken);
    }
}
