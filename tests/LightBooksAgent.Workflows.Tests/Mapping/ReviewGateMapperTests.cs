using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Mapping;

namespace LightBooksAgent.Workflows.Tests.Mapping;

public class ReviewGateMapperTests
{
    [Theory]
    [InlineData(ReviewGateType.ResearchApproval, PublishingStep.ResearchReview)]
    [InlineData(ReviewGateType.OutlineApproval, PublishingStep.OutlineReview)]
    [InlineData(ReviewGateType.DraftReview, PublishingStep.HumanDraftReview)]
    [InlineData(ReviewGateType.FinalApproval, PublishingStep.FinalApproval)]
    [InlineData(ReviewGateType.PublishConfirmation, PublishingStep.Publishing)]
    [InlineData(ReviewGateType.UrlFetchApproval, PublishingStep.ResearchUrlApproval)]
    public void ToPublishingStep_MapsKnownGates(ReviewGateType gate, PublishingStep expected) =>
        Assert.Equal(expected, ReviewGateMapper.ToPublishingStep(gate));

    [Theory]
    [InlineData(ReviewGateType.UrlFetchApproval, PublishingStep.Researching)]
    [InlineData(ReviewGateType.ResearchApproval, PublishingStep.Outlining)]
    [InlineData(ReviewGateType.OutlineApproval, PublishingStep.Writing)]
    [InlineData(ReviewGateType.DraftReview, PublishingStep.FinalApproval)]
    [InlineData(ReviewGateType.FinalApproval, PublishingStep.Publishing)]
    [InlineData(ReviewGateType.PublishConfirmation, PublishingStep.Published)]
    public void ToNextStepAfterApproval_MapsKnownGates(ReviewGateType gate, PublishingStep expected) =>
        Assert.Equal(expected, ReviewGateMapper.ToNextStepAfterApproval(gate));

    [Theory]
    [InlineData(ReviewGateType.UrlFetchApproval, PublishingStep.ResearchUrlProposing)]
    [InlineData(ReviewGateType.ResearchApproval, PublishingStep.Researching)]
    [InlineData(ReviewGateType.OutlineApproval, PublishingStep.Outlining)]
    [InlineData(ReviewGateType.DraftReview, PublishingStep.Revision)]
    [InlineData(ReviewGateType.FinalApproval, PublishingStep.HumanDraftReview)]
    [InlineData(ReviewGateType.PublishConfirmation, PublishingStep.Publishing)]
    public void ToRetryStep_MapsKnownGates(ReviewGateType gate, PublishingStep expected) =>
        Assert.Equal(expected, ReviewGateMapper.ToRetryStep(gate));

    [Fact]
    public void ToPublishingStep_UnknownGate_ReturnsProjectCreated() =>
        Assert.Equal(PublishingStep.ProjectCreated, ReviewGateMapper.ToPublishingStep(ReviewGateType.TopicConfirmation));

    [Fact]
    public void ToNextStepAfterApproval_UnknownGate_ReturnsProjectCreated() =>
        Assert.Equal(PublishingStep.ProjectCreated, ReviewGateMapper.ToNextStepAfterApproval(ReviewGateType.TopicConfirmation));

    [Fact]
    public void ToRetryStep_UnknownGate_ReturnsProjectCreated() =>
        Assert.Equal(PublishingStep.ProjectCreated, ReviewGateMapper.ToRetryStep(ReviewGateType.TopicConfirmation));
}
