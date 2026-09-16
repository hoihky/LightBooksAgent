using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Interfaces;

public interface IMemoryService
{
    Task<MemoryContext> RetrieveContextAsync(
        string agentName,
        ArticleCategory? category,
        string topic,
        CancellationToken cancellationToken = default);

    Task StoreEpisodicAsync(
        Guid articleProjectId,
        string agentName,
        string content,
        CancellationToken cancellationToken = default);

    Task StoreSemanticAsync(
        string agentName,
        ArticleCategory? category,
        string content,
        float importance = 0.5f,
        CancellationToken cancellationToken = default);

    Task DistillFromFeedbackAsync(
        Guid articleProjectId,
        string feedback,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRecentMemoriesAsync(
        string? agentName = null,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
