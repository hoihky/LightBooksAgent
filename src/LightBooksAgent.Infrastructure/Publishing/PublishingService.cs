using System.Text;
using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Constants;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LightBooksAgent.Infrastructure.Publishing;

public sealed class PublishingService(
    AppDbContext db,
    IMdWebPublisher mdWebPublisher,
    IMemoryService memoryService,
    ILocalLlmClient llmClient,
    IOptions<PublishingOptions> publishingOptions) : IPublishingService
{
    public async Task<PublishResult> PublishAsync(
        Guid articleProjectId,
        Guid publishingRunId,
        bool exportHtml = false,
        bool useWeChatTheme = false,
        CancellationToken cancellationToken = default)
    {
        var project = await db.ArticleProjects
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.Id == articleProjectId, cancellationToken);

        if (project is null)
        {
            return new PublishResult { Success = false, Errors = ["Article not found."] };
        }

        var latestVersion = project.Versions
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();

        if (latestVersion is null || string.IsNullOrWhiteSpace(latestVersion.ContentMarkdown))
        {
            return new PublishResult { Success = false, Errors = ["No article draft available to publish."] };
        }

        var articleDirectory = Path.Combine(
            publishingOptions.Value.ArticlesPath,
            project.Id.ToString("N"));

        Directory.CreateDirectory(articleDirectory);

        var markdownPath = Path.Combine(articleDirectory, "index.md");
        var frontMatter = BuildFrontMatter(project, latestVersion);
        await File.WriteAllTextAsync(
            markdownPath,
            frontMatter + latestVersion.ContentMarkdown,
            Encoding.UTF8,
            cancellationToken);

        string? htmlOutput = null;
        var errors = new List<string>();

        if (exportHtml)
        {
            var htmlDirectory = Path.Combine(
                publishingOptions.Value.ArticlesPath,
                "..",
                "exports",
                project.Id.ToString("N"));

            var htmlResult = await mdWebPublisher.GenerateHtmlAsync(
                articleDirectory,
                Path.GetFullPath(htmlDirectory),
                project.Title,
                useWeChatTheme,
                cancellationToken);

            if (!htmlResult.Success)
            {
                errors.AddRange(htmlResult.Errors);
            }
            else
            {
                htmlOutput = htmlResult.HtmlOutputPath;
            }
        }

        project.Status = ArticleStatus.Published;
        project.UpdatedAt = DateTimeOffset.UtcNow;

        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run is not null)
        {
            run.CurrentStep = PublishingStep.Published;
            run.AgentStatus = AgentRunStatus.Completed;
            run.CompletedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        await RunReflectionAsync(project, latestVersion.ContentMarkdown, cancellationToken);

        return new PublishResult
        {
            Success = errors.Count == 0,
            MarkdownPath = markdownPath,
            HtmlOutputPath = htmlOutput,
            Errors = errors
        };
    }

    private async Task RunReflectionAsync(
        Core.Entities.ArticleProject project,
        string finalDraft,
        CancellationToken cancellationToken)
    {
        var reflectionPrompt =
            $"Category: {project.Category}\n" +
            $"Topic: {project.ConfirmedTopic}\n" +
            $"Title: {project.Title}\n\n" +
            "Summarize 3 concise lessons learned from publishing this article.";

        var reflection = await llmClient.CompleteAsync(
            "You reflect on completed technical articles and extract reusable lessons.",
            reflectionPrompt + "\n\nDraft excerpt:\n" + finalDraft[..Math.Min(2000, finalDraft.Length)],
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(reflection))
        {
            await memoryService.StoreSemanticAsync(
                AgentNames.Reflection,
                project.Category,
                reflection.Trim(),
                importance: 0.7f,
                cancellationToken: cancellationToken);

            await memoryService.StoreEpisodicAsync(
                project.Id,
                AgentNames.Reflection,
                reflection.Trim(),
                cancellationToken);
        }
    }

    private static string BuildFrontMatter(
        Core.Entities.ArticleProject project,
        Core.Entities.ArticleVersion version)
    {
        var category = project.Category.ToString();
        var publishedAt = DateTimeOffset.UtcNow.ToString("O");

        return $"""
            ---
            title: {EscapeYaml(project.Title)}
            category: {category}
            audience: {EscapeYaml(project.Audience)}
            topic: {EscapeYaml(project.ConfirmedTopic ?? string.Empty)}
            version: {version.VersionNumber}
            published_at: {publishedAt}
            ---

            """;
    }

    private static string EscapeYaml(string value) =>
        value.Replace("\"", "\\\"", StringComparison.Ordinal);
}
