using LightBooksAgent.Agents.Runner;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class OutlineStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.Outlining;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agentRunner = scope.ServiceProvider.GetRequiredService<AgentRunner>();

        var project = await db.ArticleProjects.FindAsync([state.ArticleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {state.ArticleProjectId} not found.");

        var outline = await agentRunner.RunOutlineAsync(
            state.PublishingRunId,
            project.Category,
            project.ConfirmedTopic!,
            state.ResearchBrief ?? project.ResearchBrief ?? string.Empty,
            cancellationToken);

        project.OutlineMarkdown = outline;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        state.OutlineMarkdown = outline;
        await db.SaveChangesAsync(cancellationToken);
        return state;
    }
}
