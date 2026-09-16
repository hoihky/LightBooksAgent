using System.Text.Json;
using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LightBooksAgent.Application.Services;

public sealed class ArticleService(AppDbContext db) : IArticleService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public async Task<ArticleProject> CreateAsync(
        string title,
        ArticleCategory category,
        string audience,
        string seedKeywords,
        CancellationToken cancellationToken = default)
    {
        var project = new ArticleProject
        {
            Title = title.Trim(),
            Category = category,
            Audience = audience.Trim(),
            SeedKeywords = seedKeywords.Trim(),
            Status = ArticleStatus.Draft
        };

        db.ArticleProjects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public Task<ArticleProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ArticleProjects.FindAsync([id], cancellationToken).AsTask();

    public async Task<IReadOnlyList<ArticleProject>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.ArticleProjects
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task ProposeTopicsAsync(
        Guid articleId,
        IReadOnlyList<TopicProposal> topics,
        CancellationToken cancellationToken = default)
    {
        var project = await db.ArticleProjects.FindAsync([articleId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleId} not found.");

        project.ProposedTopicsJson = JsonSerializer.Serialize(topics, JsonOptions);
        project.Status = ArticleStatus.InProgress;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmTopicAsync(Guid articleId, string topic, CancellationToken cancellationToken = default)
    {
        var project = await db.ArticleProjects.FindAsync([articleId], cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleId} not found.");

        project.ConfirmedTopic = topic.Trim();
        project.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ArticleVersion> SaveVersionAsync(
        Guid articleId,
        string markdown,
        string createdBy,
        string? changeSummary = null,
        CancellationToken cancellationToken = default)
    {
        var latestVersion = await db.ArticleVersions
            .Where(v => v.ArticleProjectId == articleId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var version = new ArticleVersion
        {
            ArticleProjectId = articleId,
            VersionNumber = latestVersion + 1,
            ContentMarkdown = markdown,
            CreatedBy = createdBy,
            ChangeSummary = changeSummary
        };

        db.ArticleVersions.Add(version);

        var project = await db.ArticleProjects.FindAsync([articleId], cancellationToken);
        if (project is not null)
        {
            project.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return version;
    }
}
