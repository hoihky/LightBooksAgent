using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IWorkflowEventProcessor
{
    Task ProcessAsync(
        WorkflowSessionHandle session,
        WorkflowEvent workflowEvent,
        CancellationToken cancellationToken = default);
}
