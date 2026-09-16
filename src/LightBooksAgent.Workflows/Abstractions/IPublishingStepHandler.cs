using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IPublishingStepHandler
{
    PublishingStep Step { get; }

    Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default);
}
