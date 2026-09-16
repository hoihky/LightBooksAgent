using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Dispatching;

public sealed class PublishingStepDispatcher(IEnumerable<IPublishingStepHandler> handlers) : IPublishingStepDispatcher
{
    private readonly IReadOnlyDictionary<PublishingStep, IPublishingStepHandler> _handlers =
        handlers.ToDictionary(handler => handler.Step);

    public Task<PublishingWorkflowState> DispatchAsync(
        PublishingStep step,
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(step, out var handler))
        {
            throw new InvalidOperationException($"No handler registered for step '{step}'.");
        }

        return handler.ExecuteAsync(state, cancellationToken);
    }
}
