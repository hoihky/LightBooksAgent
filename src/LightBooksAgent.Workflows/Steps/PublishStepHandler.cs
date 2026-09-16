using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Steps;

public sealed class PublishStepHandler(IServiceScopeFactory scopeFactory) : IPublishingStepHandler
{
    public PublishingStep Step => PublishingStep.Publishing;

    public async Task<PublishingWorkflowState> ExecuteAsync(
        PublishingWorkflowState state,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var publishingService = scope.ServiceProvider.GetRequiredService<IPublishingService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var result = await publishingService.PublishAsync(
            state.ArticleProjectId,
            state.PublishingRunId,
            state.ExportHtml,
            state.UseWeChatTheme,
            cancellationToken);

        if (!result.Success)
        {
            var run = await db.PublishingRuns.FindAsync([state.PublishingRunId], cancellationToken);
            if (run is not null)
            {
                run.ErrorMessage = string.Join("; ", result.Errors);
                run.AgentStatus = AgentRunStatus.Error;
                run.CurrentStep = PublishingStep.Failed;
                await db.SaveChangesAsync(cancellationToken);
            }

            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        state.CurrentStep = PublishingStep.Published;
        return state;
    }
}
