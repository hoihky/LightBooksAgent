using LightBooksAgent.Agents.Runner;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class ResearchUrlProposalStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.ResearchUrlProposing;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agentRunner = scope.ServiceProvider.GetRequiredService<AgentRunner>();

        var project = await db.ArticleProjects.FindAsync([state.ArticleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {state.ArticleProjectId} not found.");

        var proposed = await agentRunner.ProposeResearchTargetUrlsAsync(
            state.PublishingRunId,
            project.SeedKeywords,
            cancellationToken);

        state.ProposedResearchUrls = proposed.ToList();
        state.ApprovedResearchUrls = [];
        state.CurrentStep = PublishingStep.ResearchUrlApproval;
        return state;
    }
}
