using LightBooksAgent.Agents.Runner;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Constants;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class EditorialStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.EditorialReview;

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

        var draft = state.LatestDraft ?? await GetLatestDraftAsync(db, project.Id, cancellationToken);
        var edited = await agentRunner.RunEditorAsync(
            state.PublishingRunId,
            project.Category,
            project.ConfirmedTopic!,
            draft,
            state.ResearchBrief ?? string.Empty,
            cancellationToken);

        await articleService.SaveVersionAsync(
            project.Id,
            edited,
            AgentNames.Editor,
            changeSummary: "Editorial pass",
            cancellationToken: cancellationToken);

        state.LatestDraft = edited;
        return state;
    }

    private static async Task<string> GetLatestDraftAsync(
        AppDbContext db,
        Guid articleId,
        CancellationToken cancellationToken)
    {
        var version = await db.ArticleVersions
            .Where(v => v.ArticleProjectId == articleId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return version?.ContentMarkdown ?? string.Empty;
    }
}
