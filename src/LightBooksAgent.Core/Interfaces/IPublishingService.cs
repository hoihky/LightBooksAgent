using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Interfaces;

public interface IPublishingService
{
    Task<PublishResult> PublishAsync(
        Guid articleProjectId,
        Guid publishingRunId,
        bool exportHtml = false,
        bool useWeChatTheme = false,
        CancellationToken cancellationToken = default);
}
