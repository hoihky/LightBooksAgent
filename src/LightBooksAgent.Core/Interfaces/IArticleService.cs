using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Interfaces;

public interface IArticleService
{
    Task<ArticleProject> CreateAsync(
        string title,
        ArticleCategory category,
        string audience,
        string seedKeywords,
        CancellationToken cancellationToken = default);

    Task<ArticleProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArticleProject>> ListAsync(CancellationToken cancellationToken = default);

    Task ProposeTopicsAsync(
        Guid articleId,
        IReadOnlyList<TopicProposal> topics,
        CancellationToken cancellationToken = default);

    Task ConfirmTopicAsync(Guid articleId, string topic, CancellationToken cancellationToken = default);

    Task<ArticleVersion> SaveVersionAsync(
        Guid articleId,
        string markdown,
        string createdBy,
        string? changeSummary = null,
        CancellationToken cancellationToken = default);
}
