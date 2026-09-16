using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Tests.Models;

public class PublishingWorkflowStateTests
{
    [Fact]
    public void CreateStart_SetsIdentifiersAndInitialStep()
    {
        var runId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var state = PublishingWorkflowState.CreateStart(runId, projectId);

        Assert.Equal(runId, state.PublishingRunId);
        Assert.Equal(projectId, state.ArticleProjectId);
        Assert.Equal(PublishingStep.Researching, state.CurrentStep);
        Assert.False(state.IsCompleted);
        Assert.Null(state.PendingGate);
        Assert.Equal(0, state.RevisionCount);
    }

    [Fact]
    public void DefaultProperties_AreInitialized()
    {
        var state = new PublishingWorkflowState
        {
            PublishingRunId = Guid.NewGuid(),
            ArticleProjectId = Guid.NewGuid()
        };

        Assert.Equal(PublishingStep.Researching, state.CurrentStep);
        Assert.False(state.LastReviewApproved);
        Assert.False(state.ExportHtml);
        Assert.False(state.UseWeChatTheme);
    }
}
