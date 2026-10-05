using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Workflows.Mapping;

public static class ReviewGateMapper
{
    public static PublishingStep ToPublishingStep(ReviewGateType gateType) => gateType switch
    {
        ReviewGateType.ResearchApproval => PublishingStep.ResearchReview,
        ReviewGateType.OutlineApproval => PublishingStep.OutlineReview,
        ReviewGateType.DraftReview => PublishingStep.HumanDraftReview,
        ReviewGateType.FinalApproval => PublishingStep.FinalApproval,
        ReviewGateType.PublishConfirmation => PublishingStep.Publishing,
        ReviewGateType.UrlFetchApproval => PublishingStep.ResearchUrlApproval,
        _ => PublishingStep.ProjectCreated
    };

    public static PublishingStep ToNextStepAfterApproval(ReviewGateType gateType) => gateType switch
    {
        ReviewGateType.UrlFetchApproval => PublishingStep.Researching,
        ReviewGateType.ResearchApproval => PublishingStep.Outlining,
        ReviewGateType.OutlineApproval => PublishingStep.Writing,
        ReviewGateType.DraftReview => PublishingStep.FinalApproval,
        ReviewGateType.FinalApproval => PublishingStep.Publishing,
        ReviewGateType.PublishConfirmation => PublishingStep.Published,
        _ => PublishingStep.ProjectCreated
    };

    public static PublishingStep ToRetryStep(ReviewGateType gateType) => gateType switch
    {
        ReviewGateType.UrlFetchApproval => PublishingStep.ResearchUrlProposing,
        ReviewGateType.ResearchApproval => PublishingStep.Researching,
        ReviewGateType.OutlineApproval => PublishingStep.Outlining,
        ReviewGateType.DraftReview => PublishingStep.Revision,
        ReviewGateType.FinalApproval => PublishingStep.HumanDraftReview,
        ReviewGateType.PublishConfirmation => PublishingStep.Publishing,
        _ => PublishingStep.ProjectCreated
    };
}
