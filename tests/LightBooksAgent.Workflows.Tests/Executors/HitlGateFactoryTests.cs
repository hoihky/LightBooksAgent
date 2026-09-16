using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Tests.Executors;

public class HitlGateFactoryTests
{
    private readonly HitlGateFactory _factory = new();

    [Theory]
    [InlineData(ReviewGateType.ResearchApproval)]
    [InlineData(ReviewGateType.OutlineApproval)]
    [InlineData(ReviewGateType.DraftReview)]
    [InlineData(ReviewGateType.FinalApproval)]
    [InlineData(ReviewGateType.PublishConfirmation)]
    public void CreatePort_ReturnsNonNullPort(ReviewGateType gateType)
    {
        var port = _factory.CreatePort(gateType);
        Assert.NotNull(port);
    }

    [Theory]
    [InlineData(ReviewGateType.ResearchApproval, "prepare-ResearchApproval")]
    [InlineData(ReviewGateType.DraftReview, "prepare-DraftReview")]
    public void CreatePrepareExecutor_HasExpectedId(ReviewGateType gateType, string expectedId)
    {
        var executor = _factory.CreatePrepareExecutor(gateType, state => CreateRequest(gateType, state));
        Assert.Equal(expectedId, executor.Id);
    }

    [Theory]
    [InlineData(ReviewGateType.OutlineApproval, "apply-OutlineApproval")]
    [InlineData(ReviewGateType.PublishConfirmation, "apply-PublishConfirmation")]
    public void CreateApplyExecutor_HasExpectedId(ReviewGateType gateType, string expectedId)
    {
        var executor = _factory.CreateApplyExecutor(gateType);
        Assert.Equal(expectedId, executor.Id);
    }

    private static ReviewRequestPayload CreateRequest(ReviewGateType gateType, PublishingWorkflowState state) =>
        new(gateType, state.PublishingRunId, state.ArticleProjectId, "summary", "content");
}
