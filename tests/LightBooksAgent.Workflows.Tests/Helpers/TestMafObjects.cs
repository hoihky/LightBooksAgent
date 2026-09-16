using System.Runtime.CompilerServices;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Tests.Helpers;

internal static class TestMafObjects
{
    public static WorkflowSessionHandle CreateSessionHandle(Guid runId, ExternalRequest? pendingRequest = null) =>
        new()
        {
            PublishingRunId = runId,
            SessionId = runId.ToString(),
            StreamingRun = CreateUninitialized<StreamingRun>(),
            Cancellation = new CancellationTokenSource(),
            PendingRequest = pendingRequest
        };

    public static ExternalRequest CreateExternalRequest() => CreateUninitialized<ExternalRequest>();

    private static T CreateUninitialized<T>() where T : class =>
        (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
