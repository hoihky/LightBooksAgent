using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Tests.Helpers;

namespace LightBooksAgent.Workflows.Tests.Hosting;

public class WorkflowSessionManagerTests
{
    [Fact]
    public void Register_AndTryGet_ReturnsHandle()
    {
        var manager = new WorkflowSessionManager();
        var runId = Guid.NewGuid();
        var handle = TestMafObjects.CreateSessionHandle(runId);

        manager.Register(handle);

        Assert.True(manager.TryGet(runId, out var retrieved));
        Assert.Same(handle, retrieved);
    }

    [Fact]
    public void TryGet_ReturnsFalseForUnknownRun()
    {
        var manager = new WorkflowSessionManager();
        Assert.False(manager.TryGet(Guid.NewGuid(), out _));
    }

    [Fact]
    public void Remove_UnregistersSession()
    {
        var manager = new WorkflowSessionManager();
        var runId = Guid.NewGuid();
        manager.Register(TestMafObjects.CreateSessionHandle(runId));

        manager.Remove(runId);

        Assert.False(manager.TryGet(runId, out _));
    }

    [Fact]
    public void SetPendingRequest_UpdatesHandle()
    {
        var manager = new WorkflowSessionManager();
        var runId = Guid.NewGuid();
        var handle = TestMafObjects.CreateSessionHandle(runId);
        var request = TestMafObjects.CreateExternalRequest();

        manager.Register(handle);
        manager.SetPendingRequest(runId, request, ReviewGateType.OutlineApproval);

        Assert.True(manager.TryGetPendingRequest(runId, out var pending));
        Assert.Same(request, pending);
        Assert.Equal(ReviewGateType.OutlineApproval, handle.PendingGate);
    }

    [Fact]
    public void TryGetPendingRequest_ReturnsFalseWhenNone()
    {
        var manager = new WorkflowSessionManager();
        var runId = Guid.NewGuid();
        manager.Register(TestMafObjects.CreateSessionHandle(runId));

        Assert.False(manager.TryGetPendingRequest(runId, out _));
    }

    [Fact]
    public void SetPendingRequest_IgnoresUnknownRun()
    {
        var manager = new WorkflowSessionManager();
        var request = TestMafObjects.CreateExternalRequest();

        manager.SetPendingRequest(Guid.NewGuid(), request, ReviewGateType.DraftReview);

        Assert.False(manager.TryGetPendingRequest(Guid.NewGuid(), out _));
    }
}
