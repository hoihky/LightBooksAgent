using LightBooksAgent.Agents.Runner;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Constants;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class ResearchStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.Researching;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var agentRunner = scope.ServiceProvider.GetRequiredService<AgentRunner>();

        var project = await db.ArticleProjects.FindAsync([state.ArticleProjectId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {state.ArticleProjectId} not found.");

        var seedUrls = ParseSeedUrls(project.SeedKeywords);
        var brief = await agentRunner.RunResearchAsync(
            state.PublishingRunId,
            project.Category,
            project.ConfirmedTopic!,
            project.SeedKeywords,
            seedUrls,
            cancellationToken);

        project.ResearchBrief = brief;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        state.ResearchBrief = brief;
        await db.SaveChangesAsync(cancellationToken);
        return state;
    }

    private static IReadOnlyList<string> ParseSeedUrls(string seedKeywords) =>
        seedKeywords
            .Split([' ', '\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            token.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .ToList();
}
