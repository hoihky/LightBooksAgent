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

public sealed class TechnicalReviewStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.TechnicalReview;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agentRunner = scope.ServiceProvider.GetRequiredService<AgentRunner>();
        var memoryService = scope.ServiceProvider.GetRequiredService<IMemoryService>();

        var project = await db.ArticleProjects.FindAsync([state.ArticleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {state.ArticleProjectId} not found.");

        var draft = state.LatestDraft ?? await GetLatestDraftAsync(db, project.Id, cancellationToken);
        var review = await agentRunner.RunTechnicalReviewAsync(
            state.PublishingRunId,
            project.Category,
            project.ConfirmedTopic!,
            draft,
            cancellationToken);

        await memoryService.StoreEpisodicAsync(
            project.Id,
            AgentNames.TechnicalReviewer,
            review[..Math.Min(1000, review.Length)],
            cancellationToken);

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
