using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IPublishingWorkflowFactory
{
    Workflow CreateWorkflow();
}
