using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Models;
using LightBooksAgent.Workflows.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LightBooksAgent.Workflows.Tests.Hosting;

public class PublishingWorkflowHostTests
{
    [Fact]
    public async Task ResumeAsync_ThrowsWithoutCheckpoint()
    {
        var host = CreateHost();
        var handle = TestMafObjects.CreateSessionHandle(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => host.ResumeAsync(handle));
    }

    [Fact]
    public async Task SubmitReviewResponseAsync_ThrowsWhenNoPendingRequest()
    {
        var sessionManager = new WorkflowSessionManager();
        var runId = Guid.NewGuid();
        sessionManager.Register(TestMafObjects.CreateSessionHandle(runId));

        var host = CreateHost(sessionManager);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => host.SubmitReviewResponseAsync(runId, new ReviewResponsePayload(true, null)));
    }

    private static PublishingWorkflowHost CreateHost(IWorkflowSessionManager? sessionManager = null) =>
        new(
            Substitute.For<IPublishingWorkflowFactory>(),
            Substitute.For<IWorkflowCheckpointService>(),
            sessionManager ?? new WorkflowSessionManager(),
            Substitute.For<IWorkflowEventProcessor>(),
            NullLogger<PublishingWorkflowHost>.Instance);
}
