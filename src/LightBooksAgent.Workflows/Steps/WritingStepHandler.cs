using LightBooksAgent.Agents.Runner;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Constants;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class WritingStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.Writing;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agentRunner = scope.ServiceProvider.GetRequiredService<AgentRunner>();
        var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();

        var project = await db.ArticleProjects.FindAsync([state.ArticleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {state.ArticleProjectId} not found.");

        var isRevision = state.CurrentStep == PublishingStep.Revision;
        var draft = await agentRunner.RunWriterAsync(
            state.PublishingRunId,
            project.Category,
            project.ConfirmedTopic!,
            state.OutlineMarkdown ?? project.OutlineMarkdown ?? string.Empty,
            isRevision ? state.HumanComment : null,
            cancellationToken);

        await articleService.SaveVersionAsync(
            project.Id,
            draft,
            AgentNames.Writer,
            changeSummary: isRevision ? "Revision draft" : "Agent draft",
            cancellationToken: cancellationToken);

        state.LatestDraft = draft;
        return state;
    }
}
