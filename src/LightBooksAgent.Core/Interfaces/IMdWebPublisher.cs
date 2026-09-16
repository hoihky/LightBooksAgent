using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Interfaces;

public interface IMdWebPublisher
{
    Task<PublishResult> GenerateHtmlAsync(
        string sourceDirectory,
        string outputDirectory,
        string title,
        bool useWeChatTheme = false,
        CancellationToken cancellationToken = default);
}
