using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Utilities;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Constants;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Factory;

public sealed class PublishingWorkflowFactory(
    StepExecutorFactory stepFactory,
    HitlGateFactory hitlGateFactory) : IPublishingWorkflowFactory
{
    public Workflow CreateWorkflow()
    {
        var initialize = stepFactory.CreateInitializer();
        var researchUrlProposal = stepFactory.Create(
            WorkflowExecutorIds.ResearchUrlProposal,
            PublishingStep.ResearchUrlProposing);
        var research = stepFactory.Create(WorkflowExecutorIds.Research, PublishingStep.Researching);
        var outline = stepFactory.Create(WorkflowExecutorIds.Outline, PublishingStep.Outlining);
        var writing = stepFactory.Create(WorkflowExecutorIds.Writing, PublishingStep.Writing);
        var technicalReview = stepFactory.Create(WorkflowExecutorIds.TechnicalReview, PublishingStep.TechnicalReview);
        var editorial = stepFactory.Create(WorkflowExecutorIds.Editorial, PublishingStep.EditorialReview);
        var publish = stepFactory.Create(WorkflowExecutorIds.Publish, PublishingStep.Publishing);
        var complete = stepFactory.CreateCompletionExecutor();

        var urlFetchGate = BuildGate(
            ReviewGateType.UrlFetchApproval,
            state => CreateReviewRequest(
                ReviewGateType.UrlFetchApproval,
                state,
                "Approve URLs to fetch before research",
                ResearchUrlParser.FormatForHumanReview(state.ProposedResearchUrls)));

        var researchGate = BuildGate(
            ReviewGateType.ResearchApproval,
            state => CreateReviewRequest(ReviewGateType.ResearchApproval, state, "Research brief ready for review", state.ResearchBrief));

        var outlineGate = BuildGate(
            ReviewGateType.OutlineApproval,
            state => CreateReviewRequest(ReviewGateType.OutlineApproval, state, "Outline ready for review", state.OutlineMarkdown));

        var draftGate = BuildGate(
            ReviewGateType.DraftReview,
            state => CreateReviewRequest(ReviewGateType.DraftReview, state, "Draft ready for critical review", state.LatestDraft));

        var finalGate = BuildGate(
            ReviewGateType.FinalApproval,
            state => CreateReviewRequest(ReviewGateType.FinalApproval, state, "Final approval before publishing", state.LatestDraft));

        var publishGate = BuildGate(
            ReviewGateType.PublishConfirmation,
            state => CreateReviewRequest(ReviewGateType.PublishConfirmation, state, "Confirm publish and optional HTML export", state.LatestDraft));

        return new WorkflowBuilder(initialize)
            .WithName("LightBooks Publishing Workflow")
            .WithDescription("Sequential MAF workflow with RequestPort HITL gates")
            .AddEdge(initialize, researchUrlProposal)
            .AddEdge(researchUrlProposal, urlFetchGate.Prepare)
            .AddEdge(urlFetchGate.Prepare, urlFetchGate.Port)
            .AddEdge(urlFetchGate.Port, urlFetchGate.Apply)
            .AddEdge<PublishingWorkflowState>(urlFetchGate.Apply, research, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(urlFetchGate.Apply, researchUrlProposal, static state => !state.LastReviewApproved)
            .AddEdge(research, researchGate.Prepare)
            .AddEdge(researchGate.Prepare, researchGate.Port)
            .AddEdge(researchGate.Port, researchGate.Apply)
            .AddEdge<PublishingWorkflowState>(researchGate.Apply, outline, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(researchGate.Apply, research, static state => !state.LastReviewApproved)
            .AddEdge(outline, outlineGate.Prepare)
            .AddEdge(outlineGate.Prepare, outlineGate.Port)
            .AddEdge(outlineGate.Port, outlineGate.Apply)
            .AddEdge<PublishingWorkflowState>(outlineGate.Apply, writing, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(outlineGate.Apply, outline, static state => !state.LastReviewApproved)
            .AddEdge(writing, technicalReview)
            .AddEdge(technicalReview, editorial)
            .AddEdge(editorial, draftGate.Prepare)
            .AddEdge(draftGate.Prepare, draftGate.Port)
            .AddEdge(draftGate.Port, draftGate.Apply)
            .AddEdge<PublishingWorkflowState>(draftGate.Apply, finalGate.Prepare, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(draftGate.Apply, writing, static state => !state.LastReviewApproved)
            .AddEdge(finalGate.Prepare, finalGate.Port)
            .AddEdge(finalGate.Port, finalGate.Apply)
            .AddEdge<PublishingWorkflowState>(finalGate.Apply, publish, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(finalGate.Apply, editorial, static state => !state.LastReviewApproved)
            .AddEdge(publish, publishGate.Prepare)
            .AddEdge(publishGate.Prepare, publishGate.Port)
            .AddEdge(publishGate.Port, publishGate.Apply)
            .AddEdge<PublishingWorkflowState>(publishGate.Apply, complete, static state => state.LastReviewApproved)
            .AddEdge<PublishingWorkflowState>(publishGate.Apply, publish, static state => !state.LastReviewApproved)
            .WithOutputFrom(complete)
            .Build();
    }

    private HitlGateBundle BuildGate(
        ReviewGateType gateType,
        Func<PublishingWorkflowState, ReviewRequestPayload> requestFactory) =>
        new(
            hitlGateFactory.CreatePrepareExecutor(gateType, requestFactory),
            hitlGateFactory.CreatePort(gateType),
            hitlGateFactory.CreateApplyExecutor(gateType));

    private static ReviewRequestPayload CreateReviewRequest(
        ReviewGateType gateType,
        PublishingWorkflowState state,
        string summary,
        string? content) =>
        new(
            gateType,
            state.PublishingRunId,
            state.ArticleProjectId,
            summary,
            content ?? string.Empty);

    private sealed record HitlGateBundle(
        FunctionExecutor<PublishingWorkflowState, ReviewRequestPayload> Prepare,
        RequestPort Port,
        FunctionExecutor<ReviewResponsePayload, PublishingWorkflowState> Apply);
}
