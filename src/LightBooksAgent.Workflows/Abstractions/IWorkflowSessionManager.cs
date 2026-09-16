using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Abstractions;

public interface IWorkflowSessionManager
{
    bool TryGet(Guid publishingRunId, out WorkflowSessionHandle? handle);

    void Register(WorkflowSessionHandle handle);

    void Remove(Guid publishingRunId);

    void SetPendingRequest(Guid publishingRunId, ExternalRequest request, ReviewGateType gateType);

    bool TryGetPendingRequest(Guid publishingRunId, out ExternalRequest? request);
}
