using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IPublishingStepDispatcher
{
    Task<PublishingWorkflowState> DispatchAsync(
        PublishingStep step,
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default);
}
