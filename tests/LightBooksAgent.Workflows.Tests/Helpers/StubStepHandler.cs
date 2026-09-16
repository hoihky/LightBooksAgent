using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Tests.Helpers;

internal sealed class StubStepHandler : IPublishingStepHandler
{
    private readonly Func<PublishingWorkflowState, PublishingWorkflowState>? _transform;

    public StubStepHandler(PublishingStep step, Func<PublishingWorkflowState, PublishingWorkflowState>? transform = null)
    {
        Step = step;
        _transform = transform;
    }

    public PublishingStep Step { get; }

    public Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        var result = _transform?.Invoke(state) ?? state;
        return Task.FromResult(result);
    }
}
