using LightBooksAgent.Workflows.Constants;

namespace LightBooksAgent.Workflows.Tests.Constants;

public class WorkflowExecutorIdsTests
{
    [Theory]
    [InlineData(WorkflowExecutorIds.Initialize)]
    [InlineData(WorkflowExecutorIds.Research)]
    [InlineData(WorkflowExecutorIds.Outline)]
    [InlineData(WorkflowExecutorIds.Writing)]
    [InlineData(WorkflowExecutorIds.TechnicalReview)]
    [InlineData(WorkflowExecutorIds.Editorial)]
    [InlineData(WorkflowExecutorIds.Publish)]
    [InlineData(WorkflowExecutorIds.Complete)]
    public void CoreExecutorIds_AreNonEmpty(string executorId)
    {
        Assert.False(string.IsNullOrWhiteSpace(executorId));
    }

    [Fact]
    public void ExecutorIds_AreUnique()
    {
        var ids = new[]
        {
            WorkflowExecutorIds.Initialize,
            WorkflowExecutorIds.Research,
            WorkflowExecutorIds.ResearchGate,
            WorkflowExecutorIds.Outline,
            WorkflowExecutorIds.OutlineGate,
            WorkflowExecutorIds.Writing,
            WorkflowExecutorIds.TechnicalReview,
            WorkflowExecutorIds.Editorial,
            WorkflowExecutorIds.DraftGate,
            WorkflowExecutorIds.FinalGate,
            WorkflowExecutorIds.Publish,
            WorkflowExecutorIds.PublishGate,
            WorkflowExecutorIds.Complete
        };

        Assert.Equal(ids.Length, ids.Distinct().Count());
    }
}
