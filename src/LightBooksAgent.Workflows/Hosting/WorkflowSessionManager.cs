using System.Collections.Concurrent;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Hosting;

public sealed class WorkflowSessionManager : IWorkflowSessionManager
{
    private readonly ConcurrentDictionary<Guid, WorkflowSessionHandle> _sessions = new();

    public bool TryGet(Guid publishingRunId, out WorkflowSessionHandle? handle) =>
        _sessions.TryGetValue(publishingRunId, out handle);

    public void Register(WorkflowSessionHandle handle) =>
        _sessions[handle.PublishingRunId] = handle;

    public void Remove(Guid publishingRunId) =>
        _sessions.TryRemove(publishingRunId, out _);

    public void SetPendingRequest(Guid publishingRunId, ExternalRequest request, ReviewGateType gateType)
    {
        if (_sessions.TryGetValue(publishingRunId, out var handle) && handle is not null)
        {
            handle.PendingRequest = request;
            handle.PendingGate = gateType;
        }
    }

    public bool TryGetPendingRequest(Guid publishingRunId, out ExternalRequest? request)
    {
        if (_sessions.TryGetValue(publishingRunId, out var handle) && handle?.PendingRequest is not null)
        {
            request = handle.PendingRequest;
            return true;
        }

        request = null;
        return false;
    }
}
