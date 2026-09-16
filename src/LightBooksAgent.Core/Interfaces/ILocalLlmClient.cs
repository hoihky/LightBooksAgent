using LightBooksAgent.Core.Models;

namespace LightBooksAgent.Core.Interfaces;

public interface ILocalLlmClient
{
    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);

    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

    Task<LlmHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
}
