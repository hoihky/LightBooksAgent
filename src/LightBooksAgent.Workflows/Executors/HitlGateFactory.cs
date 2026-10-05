using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Utilities;
using LightBooksAgent.Workflows.Mapping;
using LightBooksAgent.Workflows.Models;
using Microsoft.Agents.AI.Workflows;

namespace LightBooksAgent.Workflows.Executors;

public sealed class HitlGateFactory
{
    private const string StateScope = "hitl-state";

    public RequestPort CreatePort(ReviewGateType gateType) =>
        RequestPort.Create<ReviewRequestPayload, ReviewResponsePayload>($"gate-{gateType}");

    public FunctionExecutor<PublishingWorkflowState, ReviewRequestPayload> CreatePrepareExecutor(
        ReviewGateType gateType,
        Func<PublishingWorkflowState, ReviewRequestPayload> requestFactory) =>
        new(
            $"prepare-{gateType}",
            async (state, context, cancellationToken) =>
            {
                state.PendingGate = gateType;
                state.CurrentStep = ReviewGateMapper.ToPublishingStep(gateType);
                await context.QueueStateUpdateAsync(StateScope, state, cancellationToken);
                return requestFactory(state);
            },
            declareCrossRunShareable: true);

    public FunctionExecutor<ReviewResponsePayload, PublishingWorkflowState> CreateApplyExecutor(ReviewGateType gateType) =>
        new(
            $"apply-{gateType}",
            async (response, context, cancellationToken) =>
            {
                var state = await context.ReadStateAsync<PublishingWorkflowState>(StateScope, cancellationToken)
                    ?? throw new InvalidOperationException("HITL state was not found in workflow context.");

                state.LastReviewApproved = response.Approved;
                state.HumanComment = response.Comment;
                state.PendingGate = null;
                state.CurrentStep = response.Approved
                    ? ReviewGateMapper.ToNextStepAfterApproval(gateType)
                    : ReviewGateMapper.ToRetryStep(gateType);

                if (!response.Approved && gateType == ReviewGateType.DraftReview)
                {
                    state.RevisionCount++;
                }

                if (gateType == ReviewGateType.UrlFetchApproval)
                {
                    state.ApprovedResearchUrls = ResearchUrlParser
                        .ResolveApprovedUrls(state.ProposedResearchUrls, response.Approved, response.Comment)
                        .ToList();
                }

                return state;
            },
            declareCrossRunShareable: true);
}
